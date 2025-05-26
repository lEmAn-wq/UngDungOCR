using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace UngDungOCR
{
    public partial class MainForm : Form
    {
        // List chứa toàn bộ dữ liệu gốc của comboBox2
        private List<CheckComboBoxItem> originalItemsComboBox2 = new List<CheckComboBoxItem>();
        private System.Drawing.Rectangle selectionRect = System.Drawing.Rectangle.Empty;
        public MainForm()
        {
            InitializeComponent();
            //bool isBitmap = pictureBox1.Image is Bitmap;

            comboBox1.Items.Add(new CheckComboBoxItem("--Chọn kiểu chữ--"));
            comboBox1.Items.Add(new CheckComboBoxItem("Chữ in"));
            comboBox1.Items.Add(new CheckComboBoxItem("Chữ viết tay"));

            comboBox2.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDown;

            // Thêm dữ liệu gốc cho comboBox2
            originalItemsComboBox2.Add(new CheckComboBoxItem("Tiếng Anh(chữ in)"));
            originalItemsComboBox2.Add(new CheckComboBoxItem("Tiếng Việt(chữ in)"));
            originalItemsComboBox2.Add(new CheckComboBoxItem("Tiếng Trung(chữ in)"));
            originalItemsComboBox2.Add(new CheckComboBoxItem("Tiếng Việt(chữ viết tay)"));

            // Gán vào comboBox2 lần đầu
            comboBox2.Items.AddRange(originalItemsComboBox2.ToArray());

            comboBox2.Text = "--Chọn mô hình--";        // giữ text mặc định

            comboBox1.SelectedIndex = 0;
        }

        // 1. SelectedIndexChanged: chỉ xử lý khi item != null, giữ Dropdown mở, đặt lại Text
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem is CheckComboBoxItem item)
            {
                item.Checked = !item.Checked;
                comboBox2.SelectedIndex = -1;      // xóa chọn, để text không bị thay
                comboBox2.DroppedDown = true;    // giữ dropdown mở
                comboBox2.Invalidate();          // yêu cầu vẽ lại toàn bộ
            }
        }

        // 2. DrawItem: kiểm tra e.Index, lấy đúng item theo index, dùng SystemFonts.DefaultFont nếu e.Font == null
        private void comboBox2_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            // Lấy item dựa trên e.Index
            if (comboBox2.Items[e.Index] is CheckComboBoxItem item)
            {
                e.DrawBackground();

                // Vẽ checkbox
                CheckBoxRenderer.DrawCheckBox(
                    e.Graphics,
                    new System.Drawing.Point(e.Bounds.X + 2, e.Bounds.Y + 2),
                    item.Checked
                        ? System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal
                        : System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal
                );

                // Vẽ text (dùng default font nếu e.Font null)
                using (var brush = new SolidBrush(e.ForeColor))
                {
                    e.Graphics.DrawString(
                        item.Text,
                        e.Font ?? SystemFonts.DefaultFont,
                        brush,
                        e.Bounds.X + 20,
                        e.Bounds.Y + 2
                    );
                }

                e.DrawFocusRectangle();
            }
            comboBox2.Text = "--Chọn mô hình--";        // giữ text mặc định
        }

        private void Form3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                // Paste ảnh
                if (Clipboard.ContainsImage())
                {
                    System.Drawing.Image pastedImage = Clipboard.GetImage();
                    string err;
                    bool valid = ValidateImageSize(pastedImage, 50, 10000, 50, 10000, out err);
                    if (!valid)
                    {
                        MessageBox.Show(err, "Lỗi kích thước ảnh", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    //// Giải phóng ảnh cũ nếu có
                    //if (pictureBox1.Image != null)
                    //{
                    //    pictureBox1.Image.Dispose();
                    //    pictureBox1.Image = null;
                    //}

                    // Copy ảnh vì pastedImage sẽ bị Dispose sau using
                    pictureBox1.Image = pastedImage;

                    // Reset vùng chọn nếu dùng
                    selectionRect = System.Drawing.Rectangle.Empty;
                }
            }
        }


        #region === Phần vẽ và tính vùng chọn trên PictureBox ===

        private bool isDragging = false;
        private System.Drawing.Point dragStartPoint;
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = e.Location;
                selectionRect = new System.Drawing.Rectangle(e.Location, new System.Drawing.Size(0, 0));
                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                int x = Math.Min(e.X, dragStartPoint.X);
                int y = Math.Min(e.Y, dragStartPoint.Y);
                int width = Math.Abs(e.X - dragStartPoint.X);
                int height = Math.Abs(e.Y - dragStartPoint.Y);

                selectionRect = new System.Drawing.Rectangle(x, y, width, height);
                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                pictureBox1.Invalidate();
                // Bạn có thể xử lý vùng chọn ở đây, ví dụ cắt ảnh...
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (!selectionRect.IsEmpty && selectionRect.Width > 0 && selectionRect.Height > 0)
            {
                using (Pen pen = new Pen(System.Drawing.Color.Lime, 2))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawRectangle(pen, selectionRect);
                }

                // Vẽ 4 chấm xanh ở 4 góc
                int size = 8;
                Brush brush = Brushes.Lime;

                e.Graphics.FillEllipse(brush, selectionRect.Left - size / 2, selectionRect.Top - size / 2, size, size);
                e.Graphics.FillEllipse(brush, selectionRect.Right - size / 2, selectionRect.Top - size / 2, size, size);
                e.Graphics.FillEllipse(brush, selectionRect.Left - size / 2, selectionRect.Bottom - size / 2, size, size);
                e.Graphics.FillEllipse(brush, selectionRect.Right - size / 2, selectionRect.Bottom - size / 2, size, size);
            }
        }

        #endregion

        #region === Hàm tính vùng chọn trên ảnh gốc ===

        /// <summary>
        /// Trả về System.Drawing.Rectangle (trong hệ toạ độ ảnh gốc) nếu đang có vùng chọn trên PictureBox.
        /// Nếu không có vùng chọn (selectionRect.IsEmpty) thì trả về null.
        /// </summary>
        private System.Drawing.Rectangle? GetOriginalImageSelection()
        {
            // Nếu không có selection hoặc quá nhỏ, trả về null
            if (selectionRect.IsEmpty || selectionRect.Width == 0 || selectionRect.Height == 0)
                return null;

            // Kích thước ảnh gốc
            int origW = pictureBox1.Image.Width;
            int origH = pictureBox1.Image.Height;

            // Kích thước PictureBox (khu vực hiển thị ảnh)
            int pbW = pictureBox1.ClientSize.Width;
            int pbH = pictureBox1.ClientSize.Height;

            // Tính tỉ lệ scale = min(pbW/origW, pbH/origH)
            float scale = Math.Min((float)pbW / origW, (float)pbH / origH);

            // Kích thước thật của ảnh sau khi zoom để vẽ trong PictureBox
            int displayedW = (int)(origW * scale);
            int displayedH = (int)(origH * scale);

            // Tính offset (ảnh được căn giữa trong PictureBox)
            int offsetX = (pbW - displayedW) / 2;
            int offsetY = (pbH - displayedH) / 2;

            // Lấy toạ độ selectionRect so với vùng ảnh thực (loại bỏ phần letterbox)
            int xInDisp = selectionRect.X - offsetX;
            int yInDisp = selectionRect.Y - offsetY;
            int wInDisp = selectionRect.Width;
            int hInDisp = selectionRect.Height;

            // Clamp tọa độ cho nằm trong vùng ảnh thực tế hiển thị
            // Nếu vùng chọn kéo ra khỏi viền ảnh (letterbox), chúng ta cắt bớt cho nằm vừa
            xInDisp = Math.Max(0, Math.Min(xInDisp, displayedW));
            yInDisp = Math.Max(0, Math.Min(yInDisp, displayedH));
            wInDisp = Math.Max(0, Math.Min(wInDisp, displayedW - xInDisp));
            hInDisp = Math.Max(0, Math.Min(hInDisp, displayedH - yInDisp));

            // Nếu vùng chọn sau clamp bằng 0 thì coi như không có selection trên ảnh gốc
            if (wInDisp == 0 || hInDisp == 0)
                return null;

            // Chuyển về toạ độ ảnh gốc
            int xOrig = (int)(xInDisp / scale);
            int yOrig = (int)(yInDisp / scale);
            int wOrig = (int)(wInDisp / scale);
            int hOrig = (int)(hInDisp / scale);

            // Đảm bảo vẫn nằm trong bounds ảnh gốc
            if (xOrig < 0) xOrig = 0;
            if (yOrig < 0) yOrig = 0;
            if (xOrig + wOrig > origW) wOrig = origW - xOrig;
            if (yOrig + hOrig > origH) hOrig = origH - yOrig;
            if (wOrig <= 0 || hOrig <= 0)
                return null;

            return new System.Drawing.Rectangle(xOrig, yOrig, wOrig, hOrig);
        }

        #endregion

        #region === Hàm trả về ảnh gốc hoặc ảnh crop ===

        /// <summary>
        /// Nếu không có vùng chọn (trên PictureBox), trả về ảnh gốc.
        /// Nếu có vùng chọn, trả về một Bitmap đã crop từ ảnh gốc.
        /// </summary>
        private System.Drawing.Image GetImageForOcr()
        {
            System.Drawing.Image baseImage;

            // 1. Tính vùng chọn (trong tọa độ ảnh gốc)
            System.Drawing.Rectangle? rectOrig = GetOriginalImageSelection();

            // 2. Nếu rectOrig == null => không có vùng chọn => trả về ảnh gốc
            if (rectOrig == null)
                baseImage = pictureBox1.Image;
            else
            {// 3. Ngược lại, crop từ ảnh gốc
                System.Drawing.Rectangle r = rectOrig.Value;
                Bitmap cropped = new Bitmap(r.Width, r.Height);
                using (Graphics g = Graphics.FromImage(cropped))
                {
                    g.DrawImage(
                        pictureBox1.Image,
                        new System.Drawing.Rectangle(0, 0, r.Width, r.Height),   // Vẽ vào khung mới (size = vùng crop)
                        r,                                        // Vùng lấy từ ảnh gốc
                        GraphicsUnit.Pixel);
                }
                baseImage = cropped;
            }

            // 4. Kiểm tra kích thước ảnh, nếu nhỏ hơn điều kiện, resize để đảm bảo kích thước tối thiểu
            if (baseImage.Width < 50 || baseImage.Height < 50)
            {
                Resize(baseImage, 50, 50, false);
            }
            else if (baseImage.Width > 10000 || baseImage.Height > 10000)
            {
                Resize(baseImage, 10000, 10000, true);
            }
            return baseImage;
        }

        #endregion

        #region === Ví dụ sử dụng (ví dụ gọi OCR) ===

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Lấy ảnh để OCR: nếu có vùng chọn thì là vùng crop, nếu không thì ảnh gốc
            System.Drawing.Image imgToOcr = GetImageForOcr();

            Clipboard.SetImage(imgToOcr);

            // Nếu cần gọi engine OCR (ví dụ Tesseract), bạn có thể chuyển imgToOcr vào:
            // string text = MyOcrEngine.Recognize((Bitmap)imgToOcr);
            // MessageBox.Show(text);

            new KetQuaForm().ShowDialog();

            // Nếu imgToOcr không phải ảnh gốc (đã crop), bạn nên dispose sau khi dùng OCR xong:
            if (!ReferenceEquals(imgToOcr, pictureBox1.Image))
                imgToOcr.Dispose();
        }

        #endregion

        private void btnUploadImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn ảnh";
                ofd.Filter = "System.Drawing.Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var tempImage = System.Drawing.Image.FromFile(ofd.FileName);
                        string err;
                        bool valid = ValidateImageSize(tempImage, 50, 10000, 50, 10000, out err);
                        if (!valid)
                        {
                            MessageBox.Show(err, "Lỗi kích thước ảnh", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return; // Dừng, không load ảnh vào pictureBox
                        }

                        // Giải phóng ảnh cũ nếu có
                        if (pictureBox1.Image != null)
                        {
                            pictureBox1.Image.Dispose();
                            pictureBox1.Image = null;
                        }

                        // Clone ảnh từ tempImage vì tempImage sẽ bị Dispose sau using
                        pictureBox1.Image = tempImage;

                        // Reset vùng chọn nếu có
                        selectionRect = System.Drawing.Rectangle.Empty;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Không thể tải ảnh: " + ex.Message);
                    }
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = comboBox1.SelectedIndex;

            comboBox2.Items.Clear();

            if (selectedIndex == 0)
            {
                // Hiện tất cả
                comboBox2.Items.AddRange(originalItemsComboBox2.ToArray());
            }
            else if (selectedIndex == 1)
            {
                // Bỏ check tất cả mục chứa "chữ viết tay"
                foreach (var it in originalItemsComboBox2.Where(item => item.Text.Contains("chữ viết tay")))
                    it.Checked = false;

                // Lọc bỏ chữ viết tay
                var filtered = originalItemsComboBox2
                    .Where(item => !item.Text.Contains("chữ viết tay"))
                    .ToArray();
                comboBox2.Items.AddRange(filtered);
            }
            else if (selectedIndex == 2)
            {
                // Bỏ check tất cả mục chứa "chữ in"
                foreach (var it in originalItemsComboBox2.Where(item => item.Text.Contains("chữ in")))
                    it.Checked = false;

                // Lọc bỏ chữ in
                var filtered = originalItemsComboBox2
                    .Where(item => item.Text.Contains("chữ viết tay"))
                    .ToArray();
                comboBox2.Items.AddRange(filtered);
            }
        }

        public static System.Drawing.Image Resize(System.Drawing.Image image, int requiredWidth, int requiredHeight, bool enlarge)
        {
            float scale;

            float scaleX = (float)requiredWidth / image.Width;
            float scaleY = (float)requiredHeight / image.Height;
            if (enlarge)
            {
                scale = Math.Max(scaleX, scaleY);
            }
            else
            {
                scale = Math.Min(scaleX, scaleY);
            }
            int newWidth = (int)(image.Width * scale);
            int newHeight = (int)(image.Height * scale);

            Bitmap resized = new Bitmap(newWidth, newHeight);
            resized.SetResolution(image.HorizontalResolution, image.VerticalResolution); // Giữ DPI gốc

            using (Graphics g = Graphics.FromImage(resized))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return resized;
        }

        public static bool ValidateImageSize(System.Drawing.Image image, int minWidth, int maxWidth, int minHeight, int maxHeight, out string errorMessage)
        {
            errorMessage = null;

            bool widthTooSmall = image.Width < minWidth;
            bool widthTooLarge = image.Width > maxWidth;
            bool heightTooSmall = image.Height < minHeight;
            bool heightTooLarge = image.Height > maxHeight;

            // Nếu chiều cao vượt max nhưng chiều rộng nhỏ hơn min
            if (heightTooLarge && widthTooSmall)
            {
                errorMessage = $"Ảnh có chiều cao vượt mức tối đa ({maxHeight}px) nhưng chiều rộng nhỏ hơn tối thiểu ({minWidth}px).";
                return false;
            }

            // Nếu chiều rộng vượt max nhưng chiều cao nhỏ hơn min
            if (widthTooLarge && heightTooSmall)
            {
                errorMessage = $"Ảnh có chiều rộng vượt mức tối đa ({maxWidth}px) nhưng chiều cao nhỏ hơn tối thiểu ({minHeight}px).";
                return false;
            }

            return true; // Ảnh hợp lệ
        }

    }
}
