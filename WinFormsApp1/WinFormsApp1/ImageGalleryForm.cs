using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using System.Configuration;
namespace WinFormsApp1
{
    public partial class ImageGalleryForm : Form
    {
        private DataGridView dgvImages;
        private Button btnLoadData;
        private Button closeButton;

        public ImageGalleryForm()
        {
            InitializeComponent(); // Gọi phương thức do designer tạo ra
        }

        private void InitializeCustomComponents()
        {
            // Khởi tạo PictureBox
            dgvImages = new DataGridView
            {
                Width = 860,
                Height = 500,
                Location = new Point(10, 10),
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                RowTemplate = { Height = 100 },
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };
            // Cột ảnh
            var imgCol = new DataGridViewImageColumn
            {
                HeaderText = "Ảnh",
                DataPropertyName = "Image",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                Width = 120
            };
            // Cột tên ảnh
            var nameCol = new DataGridViewTextBoxColumn
            {
                HeaderText = "Tên ảnh",
                DataPropertyName = "Name",
                Width = 300
            };
            // Cột nhãn
            var labelCol = new DataGridViewTextBoxColumn
            {
                HeaderText = "Nhãn",
                DataPropertyName = "Label",
                Width = 420
            };
            dgvImages.Columns.Add(imgCol);
            dgvImages.Columns.Add(nameCol);
            dgvImages.Columns.Add(labelCol);

            btnLoadData = new Button
            {
                Text = "Tải dữ liệu ảnh",
                Location = new Point(10, 520),
                Size = new Size(150, 30)
            };
            btnLoadData.Click += BtnLoadData_Click;
            this.Controls.Add(dgvImages);
            this.Controls.Add(btnLoadData);

            // Khởi tạo nút Close
            closeButton = new Button
            {
                Text = "Close",
                Dock = DockStyle.Bottom, // Nằm ở dưới cùng
                Height = 30
            };
            closeButton.Click += CloseButton_Click; // Gán sự kiện click

            // Thêm các control vào form
            Controls.Add(closeButton);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close(); // Đóng form
        }

        // Phương thức để hiển thị ảnh (ví dụ)
        private void BtnLoadData_Click(object sender, EventArgs e)
        {
            LoadImagesFromDatabase();
        }

        private void LoadImagesFromDatabase()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["YourHeartIHave1"].ConnectionString;
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT NameImage, Label, ImageData FROM Images";
                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Columns.Add("Image", typeof(Image));
                        dt.Columns.Add("Name", typeof(string));
                        dt.Columns.Add("Label", typeof(string));
                        while (reader.Read())
                        {
                            byte[] imgBytes = reader["ImageData"] as byte[];
                            Image img = null;
                            if (imgBytes != null && imgBytes.Length > 0)
                            {
                                using (var ms = new MemoryStream(imgBytes))
                                {
                                    img = Image.FromStream(ms);
                                }
                            }
                            string name = reader["NameImage"].ToString();
                            string label = reader["Label"].ToString();
                            dt.Rows.Add(img, name, label);
                        }
                        dgvImages.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message);
            }
        }
        // Override OnLoad để gọi InitializeCustomComponents sau InitializeComponent
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitializeCustomComponents();
        }

    }
}
