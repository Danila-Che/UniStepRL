import torch
import torch.nn as nn
import torch.nn.functional as F
import torch_directml
from torch.utils.data import Dataset, DataLoader

device = torch_directml.device()
print(f"Using device: {device}")

class MultiIOmodel(nn.Module):
    def __init__(self):
        super().__init__()
        # Shared layers (example: two small conv branches + fusion)
        self.conv1 = nn.Conv2d(3, 16, 3, padding=1)
        self.conv2 = nn.Conv2d(3, 16, 3, padding=1)
        self.fc_fusion = nn.Linear(16 * 32 * 32 * 2, 128)  # assume 32x32 inputs

        # Output heads
        self.class_head = nn.Linear(128, 10)   # 10 classes
        self.regress_head = nn.Linear(128, 1)  # single value

    def forward(self, x1, x2):
        # Process each input separately
        h1 = F.relu(self.conv1(x1))
        h2 = F.relu(self.conv2(x2))

        # Flatten and concatenate
        h1 = h1.view(h1.size(0), -1)
        h2 = h2.view(h2.size(0), -1)
        h = torch.cat([h1, h2], dim=1)

        # Shared fusion
        h = F.relu(self.fc_fusion(h))

        # Outputs
        out_class = self.class_head(h)          # shape: (batch, 10)
        out_reg   = self.regress_head(h)        # shape: (batch, 1)
        return out_class, out_reg

class MultiIODataset(Dataset):
    def __init__(self, size=1000, img_size=32):
        self.size = size
        self.img_size = img_size
        # Generate synthetic data for demonstration
        self.data_x1 = torch.randn(size, 3, img_size, img_size)
        self.data_x2 = torch.randn(size, 3, img_size, img_size)
        # Synthetic targets: class labels (0..9) and regression values (float)
        self.targets_class = torch.randint(0, 10, (size,))
        self.targets_reg = torch.randn(size, 1)

    def __len__(self):
        return self.size

    def __getitem__(self, idx):
        x1 = self.data_x1[idx]
        x2 = self.data_x2[idx]
        y_class = self.targets_class[idx]
        y_reg = self.targets_reg[idx]
        return x1, x2, y_class, y_reg
    
batch_size = 32
dataset = MultiIODataset(size=2000)
dataloader = DataLoader(dataset, batch_size=batch_size, shuffle=True)

model = MultiIOmodel().to(device)

criterion_cls = nn.CrossEntropyLoss()
criterion_reg = nn.MSELoss()
optimizer = torch.optim.Adam(model.parameters(), lr=0.001)

num_epochs = 5

for epoch in range(num_epochs):
    running_loss = 0.0
    for i, (x1, x2, y_class, y_reg) in enumerate(dataloader):
        # Move data to the same device as the model
        x1 = x1.to(device)
        x2 = x2.to(device)
        y_class = y_class.to(device)
        y_reg = y_reg.to(device)

        # Forward pass
        out_class, out_reg = model(x1, x2)

        # Compute losses
        loss_cls = criterion_cls(out_class, y_class)
        loss_reg = criterion_reg(out_reg.squeeze(), y_reg.squeeze())
        loss = loss_cls + loss_reg   # total loss

        # Backward pass and optimization
        optimizer.zero_grad()
        loss.backward()
        optimizer.step()

        # Log loss after every batch
        print(f"Epoch {epoch+1}, Batch {i+1}: loss = {loss.item():.4f} "
              f"(cls: {loss_cls.item():.4f}, reg: {loss_reg.item():.4f})")

        # Optional: accumulate for epoch summary
        running_loss += loss.item()

    # Optional: average loss per epoch
    print(f"Epoch {epoch+1} average loss: {running_loss / len(dataloader):.4f}\n")
