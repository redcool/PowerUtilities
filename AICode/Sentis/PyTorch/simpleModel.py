import torch
import torch.nn as nn

# 1. Define a simple model (Inherits from nn.Module)
class SimpleAdd(nn.Module):
    def __init__(self):
        super().__init__()

    def forward(self, x, y):
        # This is where the "Tensor math" happens
        return x + y

# 2. Create the model instance
model = SimpleAdd()
model.eval() # Set to evaluation mode

# 3. Create "Dummy Input" 
# This tells PyTorch the SHAPE and TYPE of tensors the model expects
dummy_x = torch.randn(1, 3) # A 1x3 Float Tensor
dummy_y = torch.randn(1, 3)

# 4. Export to ONNX
torch.onnx.export(
    model,                      # The model
    (dummy_x, dummy_y),         # Model inputs (as a tuple)
    "simple_add.onnx",          # Where to save
    input_names=['input_x', 'input_y'],   # Name your inputs for C# access
    output_names=['output_sum'],          # Name your output
    opset_version=15            # IMPORTANT: Sentis prefers 12-15 range
)

print("ONNX model exported successfully!")