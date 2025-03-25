import base64
import mimetypes

def encode_image(image_path: str) -> str:
    """Convert image to base64 string."""
    # with open(image_path, "rb") as img_file:
    #     image_base64 = base64.b64encode(img_file.read()).decode("utf-8")
    # return f"data:image/png;base64,{image_base64}"

    mime_type, _ = mimetypes.guess_type(image_path)
    if not mime_type:
        mime_type = "application/octet-stream"

    with open(image_path, "rb") as img_file:
        image_base64 = base64.b64encode(img_file.read()).decode("utf-8")
    
    return f"data:{mime_type};base64,{image_base64}"