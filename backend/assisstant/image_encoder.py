import base64
import mimetypes
from PIL import Image
import io

def resize_image(image_path: str, max_size: int = 512) -> bytes:
    """Resize image and return compressed JPEG bytes."""
    img = Image.open(image_path)
    if img.mode == "RGBA":
        img = img.convert("RGB")

    img.thumbnail((max_size, max_size))  # Resize while keeping aspect ratio

    img_bytes = io.BytesIO()
    img.save(img_bytes, format="JPEG", quality=70)  # Convert to JPEG with compression
    return img_bytes.getvalue()

def encode_image(image_path: str) -> str:
    """Convert image to base64 string."""
  
    import base64
    import mimetypes
    
    # mime_type, _ = mimetypes.guess_type(image_path)
    # if not mime_type:
    #     mime_type = "application/octet-stream"
    
    # with open(image_path, "rb") as img_file:
    #     image_base64 = base64.b64encode(img_file.read()).decode("utf-8")

    mime_type = "image/jpeg"  # Force JPEG for better compression
    img_bytes = resize_image(image_path)
    image_base64 = base64.b64encode(img_bytes).decode("utf-8")
    
    return image_base64, mime_type