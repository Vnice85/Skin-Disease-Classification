using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.Data.SqlClient;
using Microsoft.Data;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private Label titleLabel;
        private GroupBox imageGroupBox;
        private Button btnLoadImage;
        private PictureBox pictureBox;
        private GroupBox resultGroupBox;
        private Button btnRunInference;
        private Label lblResultLabel;
        private Label lblResultClass;
        private OpenFileDialog openFileDialog;
        private Label lblStatus;
        private Button btnViewImages;

        private Image loadedImage;

        public Form1()
        {
            InitializeComponent();
            //InitializeCustomComponents();
            this.Load += Form1_Load;
        }

        private void InitializeComponent()
        {
            // Form settings
            this.Text = "Hệ thống học bán giám sát - Gán nhãn và phân loại ảnh y tế";
            this.Size = new Size(900, 650);
            this.MinimumSize = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

            // Title Label
            titleLabel = new Label();
            titleLabel.Text = "Hệ thống Học bán giám sát để gán nhãn tự động ảnh y tế và phân loại bệnh";
            titleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point);
            titleLabel.ForeColor = Color.FromArgb(0, 51, 102);
            titleLabel.AutoSize = false;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 70;
            titleLabel.Padding = new Padding(10, 20, 10, 10);

            // Image GroupBox
            imageGroupBox = new GroupBox();
            imageGroupBox.Text = "1. Nhập ảnh y tế";
            imageGroupBox.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            imageGroupBox.ForeColor = Color.FromArgb(0, 51, 102);
            imageGroupBox.Size = new Size(420, 450);
            imageGroupBox.Location = new Point(30, 100);
            imageGroupBox.Padding = new Padding(15);
            imageGroupBox.BackColor = Color.FromArgb(245, 247, 250);

            // Load Image Button
            btnLoadImage = new Button();
            btnLoadImage.Text = "Chọn ảnh...";
            btnLoadImage.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            btnLoadImage.Size = new Size(120, 38);
            btnLoadImage.Location = new Point(140, 35);
            btnLoadImage.BackColor = Color.FromArgb(0, 102, 204);
            btnLoadImage.ForeColor = Color.White;
            btnLoadImage.FlatStyle = FlatStyle.Flat;
            btnLoadImage.FlatAppearance.BorderSize = 0;
            btnLoadImage.Cursor = Cursors.Hand;
            btnLoadImage.MouseEnter += (s, e) => btnLoadImage.BackColor = Color.FromArgb(0, 82, 163);
            btnLoadImage.MouseLeave += (s, e) => btnLoadImage.BackColor = Color.FromArgb(0, 102, 204);
            btnLoadImage.Click += BtnLoadImage_Click;

            // PictureBox to show image preview
            pictureBox = new PictureBox();
            pictureBox.BorderStyle = BorderStyle.FixedSingle;
            pictureBox.BackColor = Color.White;
            pictureBox.Size = new Size(370, 370);
            pictureBox.Location = new Point(25, 90);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;

            // Add controls to imageGroupBox
            imageGroupBox.Controls.Add(btnLoadImage);
            imageGroupBox.Controls.Add(pictureBox);

            // Result GroupBox
            resultGroupBox = new GroupBox();
            resultGroupBox.Text = "2. Kết quả gán nhãn và phân loại";
            resultGroupBox.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            resultGroupBox.ForeColor = Color.FromArgb(0, 51, 102);
            resultGroupBox.Size = new Size(380, 310);
            resultGroupBox.Location = new Point(480, 100);
            resultGroupBox.Padding = new Padding(15);
            resultGroupBox.BackColor = Color.FromArgb(245, 247, 250);

            // Run Inference Button
            btnRunInference = new Button();
            btnRunInference.Text = "Chạy phân loại";
            btnRunInference.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            btnRunInference.Size = new Size(150, 40);
            btnRunInference.Location = new Point(115, 35);
            btnRunInference.BackColor = Color.FromArgb(0, 153, 102);
            btnRunInference.ForeColor = Color.White;
            btnRunInference.FlatStyle = FlatStyle.Flat;
            btnRunInference.FlatAppearance.BorderSize = 0;
            btnRunInference.Cursor = Cursors.Hand;
            btnRunInference.MouseEnter += (s, e) => btnRunInference.BackColor = Color.FromArgb(0, 112, 75);
            btnRunInference.MouseLeave += (s, e) => btnRunInference.BackColor = Color.FromArgb(0, 153, 102);
            btnRunInference.Click += BtnRunInference_Click;

            // Result Labels
            lblResultLabel = new Label();
            lblResultLabel.Text = "Nhãn được gán: ";
            lblResultLabel.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
            lblResultLabel.Location = new Point(30, 110);
            lblResultLabel.AutoSize = true;
            lblResultLabel.ForeColor = Color.FromArgb(51, 51, 51);

            lblResultClass = new Label();
            lblResultClass.Text = "Phân loại bệnh: ";
            lblResultClass.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
            lblResultClass.Location = new Point(30, 160);
            lblResultClass.AutoSize = true;
            lblResultClass.ForeColor = Color.FromArgb(51, 51, 51);

            // Status label (bottom)
            lblStatus = new Label();
            lblStatus.Text = "Trạng thái: Chưa chọn ảnh";
            lblStatus.Font = new Font("Segoe UI", 10F, FontStyle.Italic);
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Location = new Point(480, 430);
            lblStatus.Size = new Size(380, 30);

            // Add controls to resultGroupBox
            resultGroupBox.Controls.Add(btnRunInference);
            resultGroupBox.Controls.Add(lblResultLabel);
            resultGroupBox.Controls.Add(lblResultClass);

            // OpenFileDialog setup
            openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Chọn ảnh y tế";
            openFileDialog.Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";

            btnViewImages = new Button();
            btnViewImages.Text = "View Images";
            btnViewImages.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnViewImages.Size = new Size(140, 45);
            btnViewImages.Location = new Point(480, 580);
            btnViewImages.BackColor = Color.FromArgb(0, 102, 204);
            btnViewImages.ForeColor = Color.White;
            btnViewImages.FlatStyle = FlatStyle.Flat;
            btnViewImages.Cursor = Cursors.Hand;
            btnViewImages.FlatAppearance.BorderSize = 0;
            // Hover effect
            btnViewImages.MouseEnter += (s, e) => btnViewImages.BackColor = Color.FromArgb(0, 82, 163);
            btnViewImages.MouseLeave += (s, e) => btnViewImages.BackColor = Color.FromArgb(0, 102, 204);
            btnViewImages.Click += BtnViewImages_Click;

            // Add controls to Form
            this.Controls.Add(titleLabel);
            this.Controls.Add(imageGroupBox);
            this.Controls.Add(resultGroupBox);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnViewImages);
        }

        private void BtnLoadImage_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    loadedImage = Image.FromFile(openFileDialog.FileName);
                    pictureBox.Image = loadedImage;
                    lblStatus.Text = $"Trạng thái: Đã chọn ảnh: {System.IO.Path.GetFileName(openFileDialog.FileName)}";
                    lblResultLabel.Text = "Nhãn được gán: ";
                    lblResultClass.Text = "Phân loại bệnh: ";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể tải ảnh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "Trạng thái: Lỗi khi tải ảnh";
                }
            }
        }

        private void BtnRunInference_Click(object sender, EventArgs e)
        {
            if (loadedImage == null)
            {
                MessageBox.Show("Vui lòng chọn ảnh y tế trước khi phân loại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // thực hiện phân loại – gọi API model ở đây

            string[] possibleLabels = { "Bình thường", "Bệnh A", "Bệnh B", "Bệnh C" }; //các nhãn muốn gán
            string[] possibleClasses = {
                "Không có dấu hiệu bất thường",
                "Xuất hiện dạng A",
                "Xuất hiện dạng B",
                "Xuất hiện dạng C"
            };//các bệnh muốn gán

            Random rnd = new Random();
            int idx = rnd.Next(possibleLabels.Length);
            //ramdom cho demo thôi

            string label = possibleLabels[idx];
            string diseaseClass = possibleClasses[idx];
            lblResultLabel.Text = $"Nhãn được gán: {label}";
            lblResultClass.Text = $"Phân loại bệnh: {diseaseClass}";
            lblStatus.Text = "Trạng thái: Phân loại hoàn tất.";
            // Save the image to the database
            string imageName = "Image_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg"; // Generate a unique name
            SaveImageToDatabase(loadedImage, imageName, label);
        }

    private void Form1_Load(object sender, EventArgs e)
    {
        string connectionString = null;
        if (ConfigurationManager.ConnectionStrings["YourHeartIHave1"] != null)
        {
            connectionString = ConfigurationManager.ConnectionStrings["YourHeartIHave1"].ConnectionString;
        }
        else
        {
            MessageBox.Show("Không tìm thấy connection string trong app.config!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return; // Thoát khỏi hàm nếu không tìm thấy connection string
        }
        if (string.IsNullOrEmpty(connectionString))
        {
            MessageBox.Show("Connection string rỗng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return; // Thoát khỏi hàm nếu connection string rỗng
        }
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                MessageBox.Show("Kết nối thành công đến SQL Server!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Thực hiện các thao tác khác với database ở đây
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối database: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
        /*private void InitializeCustomComponents()
        {
            // Create "View Images" button
            btnViewImages = new Button
            {
                Text = "View Images",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Size = new Size(140, 45),
                Location = new Point(480, 580),
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
            };
            btnViewImages.FlatAppearance.BorderSize = 0;
            // Hover effect
            btnViewImages.MouseEnter += (s, e) => btnViewImages.BackColor = Color.FromArgb(0, 82, 163);
            btnViewImages.MouseLeave += (s, e) => btnViewImages.BackColor = Color.FromArgb(0, 102, 204);
            btnViewImages.Click += BtnViewImages_Click;
            this.Controls.Add(btnViewImages);
        }*/
        private void BtnViewImages_Click(object sender, EventArgs e)
        {
            // Open ImageGalleryForm
            ImageGalleryForm galleryForm = new ImageGalleryForm();
            galleryForm.Activate();
            galleryForm.StartPosition = FormStartPosition.CenterScreen;
            galleryForm.ShowDialog();
        }
        private void SaveImageToDatabase(Image image, string imageName, string label)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["YourHeartIHave1"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Convert Image to byte array
                    byte[] imageData;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.Save(ms, ImageFormat.Jpeg); // Or another format
                        imageData = ms.ToArray();
                    }
                    // SQL query to insert image data
                    string query = "INSERT INTO Images (NameImage, ImageData, Label) VALUES (@NameImage, @ImageData, @Label)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NameImage", imageName);
                        cmd.Parameters.AddWithValue("@Label", label);
                        // Explicitly specify the data type and value for @ImageData
                        SqlParameter imageDataParam = new SqlParameter("@ImageData", SqlDbType.VarBinary);
                        imageDataParam.Value = imageData;
                        cmd.Parameters.Add(imageDataParam);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Image saved to database successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save image to database:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

