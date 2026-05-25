using RuralHousingApp.Data;
using RuralHousingApp.Models;
using RuralHousingApp.Services;

namespace RuralHousingApp.UI
{
    public class MainForm : Form
    {
        private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true };
        private readonly TextBox _txtSearch = new() { Width = 250 };
        private readonly Button _btnNew = new() { Text = "➕ إضافة جديد", Width = 120 };
        private readonly Button _btnEdit = new() { Text = "✏️ تعديل", Width = 100 };
        private readonly Button _btnDelete = new() { Text = "🗑️ حذف", Width = 100 };
        private readonly Button _btnPdf05 = new() { Text = "📄 توليد Annexe 05", Width = 140 };
        private readonly Button _btnPdf06 = new() { Text = "📄 توليد Annexe 06", Width = 140 };

        public MainForm()
        {
            Text = "نظام إدارة الملاحق - Habitat Rural";
            Width = 1100; Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            var topPanel = new Panel { Height = 45, Dock = DockStyle.Top };
            topPanel.Controls.AddRange(new Control[] { _btnPdf06, _btnPdf05, _btnDelete, _btnEdit, _btnNew, _txtSearch });
            _txtSearch.Left = 10; _txtSearch.Top = 10; _txtSearch.PlaceholderText = "بحث بالاسم أو الكود...";
            _btnNew.Left = 270; _btnNew.Top = 10;
            _btnEdit.Left = 400; _btnEdit.Top = 10;
            _btnDelete.Left = 510; _btnDelete.Top = 10;
            _btnPdf05.Left = 620; _btnPdf05.Top = 10;
            _btnPdf06.Left = 770; _btnPdf06.Top = 10;

            Controls.Add(topPanel);
            Controls.Add(_grid);

            _txtSearch.TextChanged += (_, _) => LoadData();
            _btnNew.Click += (_, _) => ShowRecordForm(new Beneficiary());
            _btnEdit.Click += (_, _) => { if (_grid.CurrentRow?.DataBoundItem is Beneficiary b) ShowRecordForm(b); };
            _btnDelete.Click += (_, _) => DeleteSelected();
            _btnPdf05.Click += (_, _) => GeneratePdf(5);
            _btnPdf06.Click += (_, _) => GeneratePdf(6);

            LoadData();
        }

        private void LoadData()
        {
            _grid.DataSource = DbHelper.GetAll(_txtSearch.Text);
        }

        private void ShowRecordForm(Beneficiary b)
        {
            using var frm = new RecordForm(b);
            if (frm.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void DeleteSelected()
        {
            if (_grid.CurrentRow?.DataBoundItem is Beneficiary b)
            {
                if (MessageBox.Show($"حذف المستفيد {b.NomBeneficiaire}؟", "تأكيد", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    DbHelper.Delete(b.Id);
                    LoadData();
                }
            }
        }

        private void GeneratePdf(int type)
        {
            if (_grid.CurrentRow?.DataBoundItem is not Beneficiary b) return;
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, type == 5 ? "ANNEXE_05.pdf" : "ANNEXE_06.pdf");
            if (type == 5) PdfGenerator.GenerateAnnexe05(b, path);
            else PdfGenerator.GenerateAnnexe06(b, path);
            MessageBox.Show($"✅ تم التصدير: {path}");
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        }
    }
}