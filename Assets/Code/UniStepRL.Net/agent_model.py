import torch
import torch.nn as nn
import torch.nn.functional as F

class ScalarEncoder(nn.Module):
    def __init__(self, input_dim, hidden_dim, output_dim):
        super(ScalarEncoder, self).__init__()

        self.network = nn.Sequential(
            nn.Linear(input_dim, hidden_dim),
            nn.ReLU(),
            nn.Linear(hidden_dim, output_dim),
            nn.ReLU(),
        )

    def forward(self, x):
        return self.network(x)


class SpatialEncoder(nn.Module):
    def __init__(self, in_channels, out_channels, input_dim, output_dim):
        super(SpatialEncoder, self).__init__()

        self.conv = nn.Conv2d(in_channels, out_channels, kernel_size=3, stride=1, padding=1)
        self.relu1 = nn.ReLU()
        self.flatten = nn.Flatten()
        self.linear = nn.Linear(input_dim * out_channels, output_dim)
        self.relu2 = nn.ReLU()

        mask = torch.tensor([[0, 1, 1],
                             [1, 1, 1],
                             [0, 1, 1]], dtype=torch.float32)
        self.register_buffer('mask', mask.view(1, 1, 3, 3))

    def forward(self, x):
        masked_weight = self.conv.weight * self.mask
        x = F.conv2d(x, masked_weight, self.conv.bias, stride=1, padding=1)
        x = self.relu1(x)
        x = self.flatten(x)
        x = self.linear(x)
        x = self.relu2(x)
        return x


class SelectDecoder(nn.Module):
    def __init__(self, input_dim, hidden_dim, output_dim):
        super(SelectDecoder, self).__init__()

        self.network = nn.Sequential(
            nn.Linear(input_dim, hidden_dim),
            nn.ReLU(),
            nn.Linear(hidden_dim, output_dim),
        )

    def forward(self, x):
        return self.network(x)


class ValueDecoder(nn.Module):
    def __init__(self, input_dim, hidden_dim):
        super(ValueDecoder, self).__init__()

        self.network = nn.Sequential(
            nn.Linear(input_dim, hidden_dim),
            nn.ReLU(),
            nn.Linear(hidden_dim, 1),
            nn.Tanh(),
        )

    def forward(self, x):
        return self.network(x)


class SpatialDecoder(nn.Module):
    def __init__(self, input_dim, hidden_dim, width, height):
        super(SpatialDecoder, self).__init__()

        self.width = width
        self.height = height

        self.network = nn.Sequential(
            nn.Linear(input_dim, hidden_dim),
            nn.ReLU(),
            nn.Linear(hidden_dim, self.width * self.height),
        )

    def forward(self, x):
        x = self.network(x)
        x = x.view(-1, 1, self.height, self.width)

        return x


class CoreModule(nn.Module):
    def __init__(self, input_dim, hidden_dim, output_dim):
        super(CoreModule, self).__init__()

        self.network = nn.Sequential(
            nn.Linear(input_dim, hidden_dim),
            nn.ReLU(),
            nn.Linear(hidden_dim, output_dim),
            nn.ReLU(),
        )

    def forward(self, x):
        return self.network(x)


class AgentModel(nn.Module):
    def __init__(self, width, height):
        super(AgentModel, self).__init__()

        self.map_encoder = SpatialEncoder(2, 8, width * height, 64)
        self.units_encoder = SpatialEncoder(7, 16, width * height, 64)
        self.can_move_encoder = SpatialEncoder(1, 4, width * height, 32)
        # amount of gold, gold receipt and gold expenditure on units
        self.global_features_encoder = ScalarEncoder(3, 32, 32)

        self.core_module = CoreModule(64 + 64 + 32 + 32, 256, 128)

        # build tower, place unit, move unit, end turn

        self.action_type_decoder = SelectDecoder(128, 64, 4)

        self.build_tower_p0_decoder = SpatialDecoder(128, 64, width, height)

        self.place_unit_p0_decoder = SelectDecoder(128, 64, 4)
        self.place_unit_p1_decoder = SpatialDecoder(128, 64, width, height)

        self.move_unit_p0_decoder = SpatialDecoder(128, 64, width, height)
        self.move_unit_p1_decoder = SpatialDecoder(128, 64, width, height)

        # V(s)

        self.v_value = ValueDecoder(128, 64)

    def forward(self, map, units, can_move, global_features):
        map_input = self.map_encoder(map)
        uints_input = self.units_encoder(units)
        can_move_input = self.can_move_encoder(can_move)
        global_features_input = self.global_features_encoder(global_features);

        core_input = torch.cat((map_input, uints_input, can_move_input, global_features_input), dim=1)
        core_output = self.core_module(core_input);

        return (
            self.action_type_decoder(core_output),

            self.build_tower_p0_decoder(core_output),

            self.place_unit_p0_decoder(core_output),
            self.place_unit_p1_decoder(core_output),

            self.move_unit_p0_decoder(core_output),
            self.move_unit_p1_decoder(core_output),

            self.v_value(core_output)
        )
    

def save_pt_model(model, path):
    torch.save(model, path)


def load_pt_model(path):
    return torch.load(path, weights_only=False)


def save_onnx_model(model, path):
    dummy_map_input = torch.randn(1, 2, 5, 5)
    dummy_units_input = torch.randn(1, 7, 5, 5)
    dummy_can_move_input = torch.randn(1, 1, 5, 5)
    dummy_global_features_input = torch.randn(1, 3)

    torch.onnx.export(model,
                    (dummy_map_input, dummy_units_input, dummy_can_move_input, dummy_global_features_input),
                    path,
                    input_names=[
                        "map_input",
                        "units_input",
                        "can_move_input",
                        "global_features_input"
                    ],
                    output_names=[
                        "action_type_output",
                        "build_tower_p0_output",
                        "place_unit_p0_output", 
                        "place_unit_p1_output",
                        "move_unit_p0_output",
                        "move_unit_p1_output",
                        "v_value_output"
                    ])

    total_params = sum(p.numel() for p in model.parameters())
    print(f"Total Parameters: {total_params:,}")

