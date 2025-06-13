import tensorflow as tf
import numpy as np
import matplotlib.pyplot as plt
from tensorflow.keras.preprocessing import image
from tensorflow.keras.applications.efficientnet import preprocess_input
import os

IMG_SIZE = (300, 300)

# ==== Danh sách lớp đúng thứ tự ánh xạ lúc training ====
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
]

# ==== Load model đã huấn luyện ====
model = tf.keras.models.load_model('model.keras')
print("✓ Đã load xong model .keras")

# ==== Đường dẫn ảnh cần dự đoán ====
img_path = 'eczema.jpg'

# ==== Tiền xử lý ảnh ====
img = image.load_img(img_path, target_size=IMG_SIZE)
img_array = image.img_to_array(img)
img_array = preprocess_input(img_array)
img_array = np.expand_dims(img_array, axis=0)  # Shape: (1, 300, 300, 3)

# ==== Dự đoán ====
preds = model.predict(img_array)
predicted_index = np.argmax(preds[0])
confidence = np.max(preds[0])

# ==== Hiển thị kết quả ====
plt.imshow(img)
plt.axis('off')
plt.title(f"Dự đoán: {CLASS_NAMES[predicted_index]}\nĐộ tin cậy: {confidence:.2%}")
plt.show()
