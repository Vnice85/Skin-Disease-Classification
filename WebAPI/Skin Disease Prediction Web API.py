import os
import numpy as np
from PIL import Image
import tensorflow as tf
from tensorflow.keras.models import load_model
from tensorflow.keras.preprocessing.image import img_to_array
from fastapi import FastAPI, File, UploadFile, HTTPException
from fastapi.responses import JSONResponse
from pydantic import BaseModel
from typing import List
import uvicorn

# Configuration
MODEL_PATH = 'D:/HM/Skin-Disease-Classification/Model/model.keras' #Replace your model path
IMG_SIZE = (300, 300) #You can change IMG_SIZE for your model
ALLOWED_IMAGE_TYPES = {'image/jpeg', 'image/png'}

# Define realistic class names for skin diseases
CLASS_NAMES = [
    "1. Eczema",
    "10. Warts Molluscum and other Viral Infections - 2103",
    "2. Melanoma 15.75k",
    "3. Atopic Dermatitis - 1.25k",
    "4. Basal Cell Carcinoma (BCC) 3323",
    "5. Melanocytic Nevi (NV) - 7970",
    "6. Benign Keratosis-like Lesions (BKL) 2624",
    "7. Psoriasis pictures Lichen Planus and related diseases - 2k",
    "8. Seborrheic Keratoses and other Benign Tumors - 1.8k",
    "9. Tinea Ringworm Candidiasis and other Fungal Infections - 1.7k"
]#Add Class name here

# Load the trained model
try:
    model = load_model(MODEL_PATH)
    print("Model loaded successfully.")
except Exception as e:
    print(f"Error loading model: {e}")
    raise RuntimeError(f"Failed to load model: {e}")

# Preprocess image for prediction
def preprocess_image(image: Image.Image) -> np.ndarray:
    """
    Preprocess an image for model prediction.
    
    Args:
        image: PIL Image object
    
    Returns:
        Preprocessed image array ready for model input
    """
    image = image.resize(IMG_SIZE)
    image_array = img_to_array(image)
    image_array = np.expand_dims(image_array, axis=0)  # Add batch dimension
    image_array = tf.keras.applications.efficientnet.preprocess_input(image_array)
    return image_array

# Prediction function
def predict_image(image: Image.Image) -> tuple[str, float, np.ndarray]:
    """
    Predict the skin disease class for an image.
    
    Args:
        image: PIL Image object
    
    Returns:
        Tuple containing predicted class name, confidence score, and all probabilities
    """
    processed_image = preprocess_image(image)
    predictions = model.predict(processed_image, verbose=0)[0]
    predicted_class_idx = np.argmax(predictions)
    predicted_class = CLASS_NAMES[predicted_class_idx]
    confidence = float(predictions[predicted_class_idx])
    return predicted_class, confidence, predictions

# Define response model
class PredictionResponse(BaseModel):
    predicted_class: str
    confidence: float
    all_probabilities: List[float]

# Initialize FastAPI app
app = FastAPI(
    title="Skin Disease Prediction API",
    description="A web API for classifying skin diseases from images using a semi-supervised learning model.",
    version="1.0.0"
)

# Root endpoint
@app.get("/")
async def root():
    """
    Root endpoint providing a welcome message.
    """
    return {
        "message": "Welcome to the Skin Disease Prediction API. Use the /predict endpoint to upload an image for classification."
    }

# Prediction endpoint
@app.post("/predict", response_model=PredictionResponse)
async def predict(file: UploadFile = File(...)):
    """
    Predict skin disease from an uploaded image.
    
    Args:
        file: Uploaded image file
    
    Returns:
        Prediction response with class, confidence, and probability distribution
    """
    # Validate file type
    if file.content_type not in ALLOWED_IMAGE_TYPES:
        raise HTTPException(
            status_code=400,
            detail="Invalid file type. Only JPEG or PNG images are allowed."
        )
    
    try:
        # Read and process the image
        image = Image.open(file.file).convert('RGB')
        predicted_class, confidence, predictions = predict_image(image)
        
        # Prepare response
        response = {
            "predicted_class": predicted_class,
            "confidence": confidence,
            "all_probabilities": predictions.tolist()
        }
        return JSONResponse(content=response)
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Error processing image: {str(e)}"
        )
    finally:
        file.file.close()

# Run the API
if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000, log_level="info")