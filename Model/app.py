import streamlit as st
import tensorflow as tf
import numpy as np
from tensorflow.keras.preprocessing import image
from tensorflow.keras.applications.efficientnet import preprocess_input
from PIL import Image
import matplotlib.pyplot as plt
import io

# ==== Configuration ====
IMG_SIZE = (300, 300)
CLASS_NAMES = [
    "Bệnh chàm (Eczema)",
    "Mụn cóc/Nhiễm virus",
    "U hắc tố (Melanoma)",
    "Viêm da cơ địa",
    "Ung thư tế bào đáy (BCC)",
    "Nốt ruồi sắc tố (Nevi)",
    "Tổn thương sừng lành tính (BKL)",
    "Vảy nến/Liken phẳng",
    "Dày sừng tiết bã",
    "Nấm da/Nhiễm nấm"
]

# Disease information
DISEASE_INFO = {
    "Bệnh chàm (Eczema)": "Tình trạng da bị ngứa, đỏ, khô và nứt nẻ.",
    "Mụn cóc/Nhiễm virus": "Các nốt sần trên da do nhiễm virus.",
    "U hắc tố (Melanoma)": "Dạng ung thư da nguy hiểm phát triển từ tế bào sắc tố.",
    "Viêm da cơ địa": "Tình trạng da mãn tính gây ngứa và viêm.",
    "Ung thư tế bào đáy (BCC)": "Loại ung thư da phổ biến, thường là u màu da.",
    "Nốt ruồi sắc tố (Nevi)": "Nốt ruồi thông thường, thường lành tính.",
    "Tổn thương sừng lành tính (BKL)": "Tổn thương da lành tính, sần sùi, có vảy.",
    "Vảy nến/Liken phẳng": "Tình trạng viêm da gây mảng đỏ, ngứa, có vảy.",
    "Dày sừng tiết bã": "Tổn thương da lành tính thường xuất hiện ở tuổi trung niên.",
    "Nấm da/Nhiễm nấm": "Nhiễm nấm gây ngứa, đỏ và tróc vảy da."
}

# ==== Load Model ====
@st.cache_resource
def load_model():
    return tf.keras.models.load_model("model.keras")

# ==== Helper Functions ====
def create_prediction_chart(predictions, class_names):
    # Get top 3 predictions
    top_indices = np.argsort(predictions)[-3:][::-1]
    top_values = [predictions[i] * 100 for i in top_indices]
    top_classes = [class_names[i] for i in top_indices]

    # Create chart
    fig, ax = plt.subplots(figsize=(10, 5))
    bars = ax.barh(top_classes, top_values, color='#4e79a7')
    ax.set_xlim(0, 100)
    ax.set_xlabel('Độ tin cậy (%)')
    ax.set_title('Top 3 dự đoán')

    # Add percentage labels
    for bar in bars:
        width = bar.get_width()
        label_x_pos = width + 1
        ax.text(label_x_pos, bar.get_y() + bar.get_height() / 2, f'{width:.1f}%',
                va='center', fontsize=10)

    plt.tight_layout()

    # Convert chart to image
    buf = io.BytesIO()
    fig.savefig(buf, format='png', bbox_inches='tight')
    buf.seek(0)
    return buf

# ==== Streamlit Interface ====
st.set_page_config(
    page_title="Hệ thống Dự đoán Bệnh Da liễu",
    page_icon="🔍",
    layout="wide",
    initial_sidebar_state="expanded"
)

# Custom CSS
st.markdown(
    """
    <style>
        .main {
            background-color: #f8f9fa;
        }
        h1, h2, h3 {
            color: #2c3e50;
            font-family: 'Arial', sans-serif;
        }
        .stCard {
            background-color: white;
            border-radius: 10px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            padding: 20px;
            margin-bottom: 20px;
        }
        .stButton>button {
            background-color: #4e79a7;
            color: white;
            border-radius: 5px;
        }
        .css-1d391kg {
            background-color: #2c3e50;
        }
        .result-highlight {
            background-color: #e8f4f8;
            border-left: 4px solid #4e79a7;
            padding: 15px;
            border-radius: 0 8px 8px 0;
            margin: 15px 0;
        }
        .high-confidence {
            color: #27ae60;
            font-weight: bold;
        }
        .medium-confidence {
            color: #f39c12;
            font-weight: bold;
        }
        .low-confidence {
            color: #e74c3c;
            font-weight: bold;
        }
        .css-1v0mbdj {
            border: 2px dashed #4e79a7;
            border-radius: 8px;
            padding: 20px;
            background-color: #f8f9fa;
        }
    </style>
    """,
    unsafe_allow_html=True
)

# ==== Sidebar ====
with st.sidebar:
    st.title("🔍 Dự đoán Bệnh Da")
    st.markdown("---")
    st.markdown("### Giới thiệu")
    st.info(
        "Ứng dụng sử dụng AI để phân tích hình ảnh bệnh da liễu. "
        "Có thể nhận diện 10 loại bệnh da phổ biến."
    )
    st.markdown("### Hướng dẫn")
    st.markdown(
        """
        1. Tải lên ảnh tổn thương da
        2. Chờ hệ thống phân tích
        3. Xem kết quả dự đoán
        4. Tham khảo ý kiến bác sĩ
        """
    )
    st.warning("⚠️ Lưu ý: Đây chỉ là công cụ hỗ trợ, không thay thế chẩn đoán y khoa.")

# ==== Main Content ====
st.title("🔍 Hệ thống Dự đoán Bệnh Da liễu bằng AI")
st.markdown("Tải lên hình ảnh tổn thương da để được phân tích tự động")

# Create tabs
tab1, tab2, tab3 = st.tabs(["📤 Tải ảnh & Dự đoán", "ℹ️ Thông tin bệnh", "❓ Hỗ trợ"])

with tab1:
    # Image upload section
    st.markdown("### Tải lên hình ảnh")
    col1, col2 = st.columns([1, 1])

    with col1:
        st.markdown('<div class="stCard">', unsafe_allow_html=True)
        uploaded_file = st.file_uploader("Chọn ảnh tổn thương da", type=["jpg", "jpeg", "png"])

        if uploaded_file:
            # Display uploaded image
            img = Image.open(uploaded_file)
            st.image(img, caption="Ảnh đã tải lên", use_column_width=True)
        st.markdown('</div>', unsafe_allow_html=True)

    with col2:
        st.markdown('<div class="stCard">', unsafe_allow_html=True)
        st.markdown("### Kết quả phân tích")

        if uploaded_file:
            # Process image for prediction
            with st.spinner("Đang phân tích ảnh..."):
                try:
                    model = load_model()

                    # Preprocess image
                    img_resized = img.resize(IMG_SIZE)
                    img_array = image.img_to_array(img_resized)
                    img_array = preprocess_input(img_array)
                    img_array = np.expand_dims(img_array, axis=0)

                    # Prediction
                    preds = model.predict(img_array)
                    predicted_index = np.argmax(preds[0])
                    confidence = np.max(preds[0]) * 100

                    # Display results
                    st.markdown("#### Kết quả dự đoán")
                    st.markdown('<div class="result-highlight">', unsafe_allow_html=True)
                    st.markdown(f"**Tình trạng dự đoán:** {CLASS_NAMES[predicted_index]}")

                    # Confidence level
                    confidence_html = ""
                    if confidence > 80:
                        confidence_html = f'<span class="high-confidence">{confidence:.1f}%</span>'
                    elif confidence > 50:
                        confidence_html = f'<span class="medium-confidence">{confidence:.1f}%</span>'
                    else:
                        confidence_html = f'<span class="low-confidence">{confidence:.1f}%</span>'

                    st.markdown(f"**Độ tin cậy:** {confidence_html}", unsafe_allow_html=True)
                    st.markdown('</div>', unsafe_allow_html=True)

                    # Display disease information
                    st.markdown("#### Thông tin về bệnh")
                    st.info(DISEASE_INFO[CLASS_NAMES[predicted_index]])

                    # Prediction chart
                    chart_image = create_prediction_chart(preds[0], CLASS_NAMES)
                    st.image(chart_image, caption="Mức độ tin cậy của các dự đoán", use_column_width=True)

                    st.success("Phân tích hoàn tất! Vui lòng tham khảo ý kiến bác sĩ để có chẩn đoán chính xác.")

                except Exception as e:
                    st.error(f"Có lỗi xảy ra: {str(e)}")
        else:
            st.info("⏳ Vui lòng tải lên ảnh để xem kết quả dự đoán.")
        st.markdown('</div>', unsafe_allow_html=True)

with tab2:
    st.markdown("### Thông tin các bệnh da liễu")
    st.markdown('<div class="stCard">', unsafe_allow_html=True)

    # Expandable sections for each disease
    for disease in CLASS_NAMES:
        with st.expander(disease):
            st.markdown(f"**{disease}**")
            st.markdown(DISEASE_INFO[disease])
            st.markdown("---")
            st.markdown("*Lưu ý: Luôn tham khảo ý kiến bác sĩ da liễu để được chẩn đoán và điều trị chính xác.*")

    st.markdown('</div>', unsafe_allow_html=True)

with tab3:
    st.markdown("### Hỗ trợ & Câu hỏi thường gặp")
    st.markdown('<div class="stCard">', unsafe_allow_html=True)

    st.markdown("#### Câu hỏi thường gặp")

    with st.expander("Loại ảnh nào cho kết quả tốt nhất?"):
        st.markdown("""
        Để có kết quả tốt nhất:
        - Ảnh rõ nét, đủ sáng
        - Tổn thương da nằm ở trung tâm
        - Tránh bóng tối hoặc chói sáng
        - Nên có phần da lành xung quanh để so sánh
        - Ảnh không bị mờ
        """)

    with st.expander("Độ chính xác của công cụ này?"):
        st.markdown("""
        Công cụ cung cấp ước lượng dựa trên dữ liệu huấn luyện, không thay thế chẩn đoán y khoa.
        Độ chính xác phụ thuộc vào chất lượng ảnh và độ rõ của tổn thương.
        Luôn tham khảo ý kiến bác sĩ chuyên khoa.
        """)

    with st.expander("Dữ liệu của tôi có được bảo mật?"):
        st.markdown("""
        Có, ảnh bạn tải lên được xử lý cục bộ và không lưu trữ vĩnh viễn.
        Ứng dụng không chia sẻ dữ liệu với bên thứ ba.
        """)

    with st.expander("Tôi nên làm gì sau khi có kết quả dự đoán?"):
        st.markdown("""
        1. Xem kết quả như thông tin tham khảo
        2. Đến gặp bác sĩ da liễu
        3. Có thể chia sẻ kết quả này với bác sĩ
        4. Không tự chẩn đoán hoặc tự điều trị
        """)

    st.markdown('</div>', unsafe_allow_html=True)

# Footer
st.markdown("---")
st.markdown(
    """
    <div style="text-align: center; color: #7f8c8d; font-size: 0.8rem;">
        Hệ thống Dự đoán Bệnh Da liễu | Công cụ hỗ trợ | Không thay thế chẩn đoán y khoa
    </div>
    """,
    unsafe_allow_html=True
)
