import torch
from torch.utils.data import Dataset, DataLoader
import torch.nn.functional as F

import struct
import numpy as np

from agent_model import AgentModel, load_pt_model, save_pt_model, save_onnx_model

def read_array_1d(f):
    size = struct.unpack("i", f.read(4))[0]
    return np.fromfile(f, dtype=np.float32, count=size)


def read_array_2d(f):
    h = struct.unpack("i", f.read(4))[0]
    w = struct.unpack("i", f.read(4))[0]
    data = np.fromfile(f, dtype=np.float32, count=h*w)
    return data.reshape(h,w)


def load_data(path):
    with open(path, "rb") as f:
        N = struct.unpack("i", f.read(4))[0]

        maps = []
        units = []
        can_move = []
        global_feat = []

        action_type = []
        build_p0 = []
        place_p0 = []
        place_p1 = []
        move_p0 = []
        move_p1 = []

        values = []

        for _ in range(N):

            maps.append(read_array_1d(f))
            units.append(read_array_1d(f))
            can_move.append(read_array_1d(f))
            global_feat.append(read_array_1d(f))

            action_type.append(read_array_1d(f))

            build_p0.append(read_array_2d(f))
            place_p0.append(read_array_1d(f))
            place_p1.append(read_array_2d(f))
            move_p0.append(read_array_2d(f))
            move_p1.append(read_array_2d(f))

            values.append(struct.unpack("f", f.read(4))[0])

    return {
        "map": np.array(maps),
        "units": np.array(units),
        "can_move": np.array(can_move),
        "global": np.array(global_feat),

        "action_type": np.array(action_type),
        "build_p0": np.array(build_p0),
        "place_p0": np.array(place_p0),
        "place_p1": np.array(place_p1),
        "move_p0": np.array(move_p0),
        "move_p1": np.array(move_p1),
        "value": np.array(values),
    }


class RLDataset(Dataset):
    def __init__(self, data):
        self.map = torch.from_numpy(data["map"]).float().view(-1,2,5,5)
        self.units = torch.from_numpy(data["units"]).float().view(-1,7,5,5)
        self.can_move = torch.from_numpy(data["can_move"]).float().view(-1,1,5,5)

        self.global_features = torch.from_numpy(data["global"]).float()

        self.action_type = torch.from_numpy(data["action_type"]).float()
        self.place_p0 = torch.from_numpy(data["place_p0"]).float()

        self.build_p0 = torch.from_numpy(data["build_p0"]).float().unsqueeze(1)
        self.place_p1 = torch.from_numpy(data["place_p1"]).float().unsqueeze(1)
        self.move_p0 = torch.from_numpy(data["move_p0"]).float().unsqueeze(1)
        self.move_p1 = torch.from_numpy(data["move_p1"]).float().unsqueeze(1)

        self.value = torch.from_numpy(data["value"]).float().unsqueeze(1)

    def __len__(self):
        return self.map.shape[0]

    def __getitem__(self, idx):

        inputs = (
            self.map[idx],
            self.units[idx],
            self.can_move[idx],
            self.global_features[idx],
        )

        targets = (
            self.action_type[idx],

            self.build_p0[idx],

            self.place_p0[idx],
            self.place_p1[idx],

            self.move_p0[idx],
            self.move_p1[idx],

            self.value[idx]
        )

        return inputs, targets


def compute_loss(
    action_type, build_p0, place_p0, place_p1, move_p0, move_p1, value,

    action_type_t, build_p0_t, place_p0_t, place_p1_t, move_p0_t, move_p1_t, value_t
):
    action_type_t = torch.argmax(action_type_t, dim=1)

    loss_action = F.cross_entropy(action_type, action_type_t)

    loss_build = F.mse_loss(build_p0, build_p0_t)

    place_p0_t = torch.argmax(place_p0_t, dim=1)
    loss_place_p0 = F.cross_entropy(place_p0, place_p0_t)
    loss_place_p1 = F.mse_loss(place_p1, place_p1_t)

    loss_move_p0 = F.mse_loss(move_p0, move_p0_t)
    loss_move_p1 = F.mse_loss(move_p1, move_p1_t)

    loss_value = F.mse_loss(value, value_t)

    return (
        loss_action +
        loss_build +
        loss_place_p0 +
        loss_place_p1 +
        loss_move_p0 +
        loss_move_p1 +
        loss_value
    )


def main():
    path = "C:/Users/galiu/Unity Projects/UniStepRL/Assets/dataset.bin"

    data = load_data(path)

    batch_size = 256
    batch_count = 16
    epoches = 10

    dataset = RLDataset(data)

    loader = DataLoader(
        dataset,
        batch_size=batch_size,
        shuffle=True,
        num_workers=0,
        pin_memory=False,
    )

    device = "cpu"
    print(f"Using device: {device}")

    pt_path = "simple_environment_agent_model.pt2"
    onnx_path = "simple_environment_agent_model.onnx"

    # model = AgentModel(5,5).to(device)
    model = load_pt_model(pt_path).to(device)
    model.train()

    optimizer = torch.optim.Adam(model.parameters(), lr=1e-4)

    print("Dataset size:", len(dataset))
    print("Map shape:", dataset.map.shape)

    for epoch in range(epoches):
        for batch_idx, (inputs, targets) in enumerate(loader):

            if batch_idx >= batch_count:
                break

            map, units, can_move, global_features = [x.to(device) for x in inputs]

            (
                action_type_t,
                build_p0_t,
                place_p0_t,
                place_p1_t,
                move_p0_t,
                move_p1_t,
                value_t
            ) = [x.to(device) for x in targets]

            outputs = model(map, units, can_move, global_features)

            (
                action_type,
                build_p0,
                place_p0,
                place_p1,
                move_p0,
                move_p1,
                value
            ) = outputs

            loss = compute_loss(
                action_type, build_p0, place_p0, place_p1,
                move_p0, move_p1, value,

                action_type_t, build_p0_t, place_p0_t,
                place_p1_t, move_p0_t, move_p1_t, value_t
            )

            optimizer.zero_grad()
            loss.backward()
            optimizer.step()

            print("epoch", epoch, "step", batch_idx, "loss", loss.item())

    save_pt_model(model, pt_path)
    save_onnx_model(model, onnx_path)

if __name__ == "__main__":
    main()
