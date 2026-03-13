import torch
import torch.nn as nn
import torch.nn.functional as F
# from torchvision.transforms.functional import rgb_to_grayscale

def rgb_to_grayscale(img):
    # 假设通道顺序是 RGB (在倒数第3维)
    # 定义权重系数
    weights = torch.tensor([0.2989, 0.5870, 0.1140], device=img.device)
    
    # 将权重与通道维进行点积运算
    # 适用于 [3, H, W] 或 [B, 3, H, W]
    if img.ndim == 3:
        # [3, H, W] -> [1, H, W]
        return (img * weights.view(3, 1, 1)).sum(dim=0, keepdim=True)
    elif img.ndim == 4:
        # [B, 3, H, W] -> [B, 1, H, W]
        return (img * weights.view(1, 3, 1, 1)).sum(dim=1, keepdim=True)
    return img

class EdgeDetection(nn.Module):
    def __init__(self):
        super().__init__()
        sobel_x = torch.tensor([[-1, 0, 1], [-2, 0, 2], [-1, 0, 1]], dtype=torch.float32)
        sobel_y = torch.tensor([[-1, -2, -1], [0, 0, 0], [1, 2, 1]], dtype=torch.float32)
        
        self.register_buffer('sobel_x', sobel_x.unsqueeze(0).unsqueeze(0))
        self.register_buffer('sobel_y', sobel_y.unsqueeze(0).unsqueeze(0))

    def forward(self, x):
        if x.size(1) == 3:
            x = rgb_to_grayscale(x)
        
        edges_x = F.conv2d(x, self.sobel_x, padding=1)
        edges_y = F.conv2d(x, self.sobel_y, padding=1)
        edges = torch.sqrt(edges_x ** 2 + edges_y ** 2)
        
        # edges = edges / edges.max() if edges.max() > 0 else edges
        edges = edges/edges.max().clamp(min=1e-8)
        
        output = edges.repeat(1, 3, 1, 1)
        
        return output


if __name__ == '__main__':
    model = EdgeDetection()
    model.eval()
    
    dummy_input = torch.randn(1, 3, 256, 256)
    
    torch.onnx.export(
        model,
        dummy_input,
        'edge_detection.onnx',
        input_names=['input'],
        output_names=['output'],
        dynamic_axes={'input': {0: 'batch', 2: 'height', 3: 'width'}, 
                      'output': {0: 'batch', 2: 'height', 3: 'width'}}
    )
    
    print("ONNX file saved to edge_detection.onnx")
    
    # import cv2
    # import numpy as np
    
    # img = cv2.imread('test.jpg')
    # if img is not None:
    #     img_tensor = torch.from_numpy(img).permute(2, 0, 1).unsqueeze(0).float() / 255.0
    #     with torch.no_grad():
    #         output = model(img_tensor)
    #     result = (output.squeeze().permute(1, 2, 0).numpy() * 255).astype(np.uint8)
    #     cv2.imwrite('result.jpg', result)
    #     print("Result saved to result.jpg")
