using System;
using System.Data;
using System.Drawing;
using System.IO; // Wajib ditambahkan untuk memproses Gambar (MemoryStream) dan File Excel
using System.Windows.Forms;
using System.Data.SqlClient;
using ExcelDataReader; // Wajib ditambahkan untuk membaca Excel

namespace CRUDMahasiswaADO
{
    public partial class Form1 : Form
    {
        // 1. Deklarasikan object DAL untuk mengambil semua fungsi database
        DAL dbLogic = new DAL();

        private BindingSource bindingSource = new BindingSource();
        private DataTable dtMahasiswa = new DataTable();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbJK.DataSource = new string[] { "L", "P" };

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Mengambil data menggunakan DAL
                dtMahasiswa = dbLogic.GetMhs();
                bindingSource.DataSource = dtMahasiswa;
                dataGridView1.DataSource = bindingSource;

                if (bindingNavigator1 != null)
                {
                    bindingNavigator1.BindingSource = bindingSource;
                }

                // Menyesuaikan ukuran kolom foto pada grid
                if (dataGridView1.Columns["Foto"] != null)
                {
                    DataGridViewImageColumn fotoColumn = (DataGridViewImageColumn)dataGridView1.Columns["Foto"];
                    fotoColumn.ImageLayout = DataGridViewImageCellLayout.Stretch;
                }

                BindControls();
                HitungTotal();

                // Mengatur status tombol saat data dimuat
                dataGridView1.Enabled = true;
                btnImpDb.Enabled = false;
                btnInsert.Enabled = true;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
                btnCari.Enabled = true;
                btnLoad.Enabled = true;
                btnResetData.Enabled = true;
                btnTestInjection.Enabled = true;
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void HitungTotal()
        {
            try
            {
                // Menghitung menggunakan fungsi CountMhs di DAL
                int total = (dbLogic.CountMhs() == 0) ? 0 : dbLogic.CountMhs();
                lblTotal.Text = "Total Mahasiswa: " + total.ToString();
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Gagal menghitung total: " + ex.Message);
            }
        }

        private void BindControls()
        {
            txtNIM.DataBindings.Clear();
            txtNama.DataBindings.Clear();
            cmbJK.DataBindings.Clear();
            dtpTanggalLahir.DataBindings.Clear();
            txtAlamat.DataBindings.Clear();
            txtKodeProdi.DataBindings.Clear();

            txtNIM.DataBindings.Add("Text", bindingSource, "NIM");
            txtNama.DataBindings.Add("Text", bindingSource, "Nama");
            cmbJK.DataBindings.Add("Text", bindingSource, "JenisKelamin");
            dtpTanggalLahir.DataBindings.Add("Value", bindingSource, "TanggalLahir");
            txtAlamat.DataBindings.Add("Text", bindingSource, "Alamat");
            txtKodeProdi.DataBindings.Add("Text", bindingSource, "KodeProdi");
        }

        private void ClearForm()
        {
            txtNIM.Enabled = true;
            txtNIM.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            txtAlamat.Clear();
            txtKodeProdi.Clear();
            dtpTanggalLahir.Value = DateTime.Now;
            fotoMhs.Image = null; // Kosongkan foto
            txtNIM.Focus();
        }

        // Method mengganti fungsi SimpanLog lama agar menggunakan DAL
        private void SimpanLog(string pesan)
        {
            dbLogic.InsertLog(pesan);
        }

        // Method konversi Gambar ke Byte Array (BLOB)
        private byte[] ConvertImageToBytes(PictureBox pb)
        {
            if (pb.Image == null) return null;
            using (MemoryStream ms = new MemoryStream())
            {
                pb.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Koneksi berhasil. Data dikelola penuh oleh DAL.");
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (txtNIM.Text == "") { MessageBox.Show("NIM harus diisi"); txtNIM.Focus(); return; }
            if (txtNama.Text == "") { MessageBox.Show("Nama harus diisi"); txtNama.Focus(); return; }
            if (cmbJK.Text == "") { MessageBox.Show("Jenis Kelamin harus dipilih"); cmbJK.Focus(); return; }
            if (txtKodeProdi.Text == "") { MessageBox.Show("Kode Prodi harus diisi"); txtKodeProdi.Focus(); return; }

            try
            {
                byte[] imgBytes = ConvertImageToBytes(fotoMhs);
                dbLogic.InsertMhs(txtNIM.Text, txtNama.Text, txtAlamat.Text, cmbJK.Text, dtpTanggalLahir.Value.Date, txtKodeProdi.Text, imgBytes);

                MessageBox.Show("Data mahasiswa berhasil ditambahkan");
                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                SimpanLog("General Error: " + ex.Message);
                MessageBox.Show("General Error: " + ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] imgBytes = ConvertImageToBytes(fotoMhs);
                dbLogic.UpdateMhs(txtNIM.Text, txtNama.Text, txtAlamat.Text, cmbJK.Text, dtpTanggalLahir.Value.Date, txtKodeProdi.Text, imgBytes);

                MessageBox.Show("Data mahasiswa berhasil diubah");
                ClearForm();
                LoadData();
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error: " + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult res = MessageBox.Show("Yakin ingin menghapus data?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    dbLogic.DeleteMhs(txtNIM.Text);
                    MessageBox.Show("Data mahasiswa berhasil dihapus");
                    ClearForm();
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error: " + ex.Message);
            }
        }

        private void btnResetData_Click(object sender, EventArgs e)
        {
            try
            {
                dbLogic.resetData();
                MessageBox.Show("Data berhasil direset");
                LoadData();
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error: " + ex.Message);
            }
        }

        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            try
            {
                dbLogic.testInject(txtNIM.Text);
                LoadData();
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("safe"))
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("SQL Error: Unsafe UPDATE operation not allowed");
                }
                else
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("SQL Error: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error: " + ex.Message);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataRow row = ((DataRowView)bindingSource[e.RowIndex]).Row;

                // Menampilkan gambar saat baris di klik
                if (row["Foto"] != DBNull.Value)
                {
                    byte[] imgBytes = (byte[])row["Foto"];
                    using (MemoryStream ms = new MemoryStream(imgBytes))
                    {
                        fotoMhs.Image = Image.FromStream(ms);
                        fotoMhs.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                }
                else
                {
                    fotoMhs.Image = null;
                }
                txtNIM.Enabled = false;
            }
        }

        private void btnCari_Click(object sender, EventArgs e)
        {
            bindingSource.DataSource = dbLogic.GetMhsByNIM(txtNIM.Text);
            dataGridView1.DataSource = bindingSource;
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                fotoMhs.Image = Image.FromFile(ofd.FileName);
                fotoMhs.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearForm();
            LoadData();
        }

        private void btnImpExcel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Excel Workbook|*.xlsx" })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                            });

                            DataTable dt = result.Tables[0];

                            // Putuskan binding sebelumnya agar grid bisa menampilkan data excel sementara
                            dataGridView1.DataSource = null;
                            dataGridView1.DataSource = dt;

                            // Matikan tombol selain Import DB agar data excel tidak tertimpa/terhapus tanpa disengaja
                            dataGridView1.Enabled = false;
                            btnImpDb.Enabled = true;
                            btnInsert.Enabled = false;
                            btnUpdate.Enabled = false;
                            btnDelete.Enabled = false;
                            btnCari.Enabled = false;
                            btnLoad.Enabled = false;
                            btnResetData.Enabled = false;
                            btnTestInjection.Enabled = false;
                        }
                    }
                }
            }
        }

        private void btnImpDb_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = (DataTable)dataGridView1.DataSource;
                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diimport.");
                    return;
                }

                int sukses = 0;
                foreach (DataRow row in dt.Rows)
                {
                    string nim = row["NIM"].ToString().Trim();
                    string nama = row["Nama"].ToString().Trim();
                    string jk = row["JenisKelamin"].ToString().Trim();
                    string alamat = row["Alamat"].ToString().Trim();
                    string kodeProdi = row["NamaProdi"].ToString().Trim();

                    if (string.IsNullOrEmpty(nim) || string.IsNullOrEmpty(nama)) continue;

                    DateTime tglLahir;
                    if (!DateTime.TryParse(row["TanggalLahir"].ToString(), out tglLahir)) continue;

                    string fotoPath = dt.Columns.Contains("FotoPath") ? row["FotoPath"].ToString().Trim() : string.Empty;
                    byte[] fotoBytes = null;

                    if (!string.IsNullOrWhiteSpace(fotoPath) && File.Exists(fotoPath))
                    {
                        fotoBytes = File.ReadAllBytes(fotoPath);
                    }

                    dbLogic.InsertMhs(nim, nama, alamat, jk, tglLahir, kodeProdi, fotoBytes);
                    sukses++;
                }

                MessageBox.Show($"Data mahasiswa berhasil diimport. Total {sukses} data.");
                ClearForm();
                LoadData(); // Load ulang dan kembalikan DataSource ke BindingSource database
            }
            catch (Exception ex)
            {
                SimpanLog("General Error: " + ex.Message);
                MessageBox.Show("General Error: " + ex.Message);
            }
        }

        private void btnRekapData_Click(object sender, EventArgs e)
        {
            Form2 frmRekap = new Form2();
            frmRekap.Show();
            this.Hide();
        }

        private void fotoMhs_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label6_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
    }
}