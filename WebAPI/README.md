# Skin Disease Prediction API

## Overview

This project implements a web API for classifying skin diseases from images using a semi-supervised learning model. The API is built with FastAPI and leverages a pre-trained TensorFlow model to predict skin disease classes, confidence scores, and probability distributions. It supports image uploads in JPEG, PNG formats and provides a RESTful interface for integration into applications.

The API is designed to classify images into one of ten skin disease categories:
- Eczema
- Melanoma
- Atopic Dermatitis
- Basal Cell Carcinoma
- Melanocytic Nevi
- Benign Keratosis-like Lesions
- Psoriasis pictures Lichen Planus and related diseases
- Seborrheic Keratoses and other Benign Tumors
- Tinea Ringworm Candidias
- Warts Molluscum and other Viral Infections

## Prerequisites

To run the API, ensure the following are installed:
- Python 3.9 or higher
- A pre-trained semi-supervised TensorFlow model (`skin_disease_semi_supervised_model.h5`)
- A compatible operating system (Windows, macOS, or Linux)

## Installation

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/Vnice85/Skin-Disease-Classification
   cd WebAPI
   ```

2. **Create a Virtual Environment**:
   ```bash
   python -m venv venv
   source venv/bin/activate  # On Windows: venv\Scripts\activate
   ```

3. **Install Dependencies**:
   Ensure the `requirements.txt` file is in the project directory, then run:
   ```bash
   pip install -r requirements.txt
   ```

4. **Prepare the Model**:
   Place the pre-trained model file (`skin_disease_semi_supervised_model.h5`) or update the `MODEL_PATH` variable in the script to point to the correct location.

## Running the API

To start the API locally:
```bash
python main.py
```

The API will be available at `http://localhost:8000`. Use a tool like `curl`, Postman, or a web browser to interact with it.

## API Endpoints

### Root Endpoint
- **URL**: `GET /`
- **Description**: Returns a welcome message.
- **Response**:
  ```json
  {
    "message": "Welcome to the Skin Disease Prediction API. Use the /predict endpoint to upload an image for classification."
  }
  ```

### Prediction Endpoint
- **URL**: `POST /predict`
- **Description**: Uploads an image and returns the predicted skin disease class, confidence score, and probability distribution.
- **Parameters**:
  - `file`: Image file (JPEG, PNG, or GIF)
- **Example Request** (using `curl`):
  ```bash
  curl -X POST "http://localhost:8000/predict" -F "file=@skin_image.jpg"
  ```
- **Example Response**:
  ```json
  {
    "predicted_class": "Melanoma",
    "confidence": 0.95,
    "all_probabilities": [0.01, 0.02, 0.01, 0.01, 0.95, 0.0, 0.0, 0.0, 0.0, 0.0]
  }
  ```

## Error Handling

- **Invalid File Type**:
  - Status Code: 400
  - Response: `{"detail": "Invalid file type. Only JPEG, PNG images are allowed."}`
- **Processing Error**:
  - Status Code: 500
  - Response: `{"detail": "Error processing image: <error-message>"}`

## Notes

- **Model Assumptions**: The API assumes the model is trained with a semi-supervised learning approach and is compatible with EfficientNet preprocessing. Ensure the model’s class names match the `CLASS_NAMES` list in the script.
- **Deployment**: For production, consider adding authentication, rate limiting, and HTTPS. Deploy using a WSGI server like Gunicorn with Uvicorn workers.
- **Scalability**: The API is designed for single-image predictions. For batch processing, extend the `/predict` endpoint to handle multiple files.

## Troubleshooting

- **Model Loading Error**: Verify the model file exists at the specified `MODEL_PATH` and is not corrupted.
- **Dependency Issues**: Ensure all packages in `requirements.txt` are installed correctly. Use `pip list` to check versions.
- **Port Conflicts**: If port 8000 is in use, modify the `port` parameter in the `uvicorn.run` call.