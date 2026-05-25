using RuralHousingApp.Models;

namespace RuralHousingApp.UI
{
    public class RecordForm : Form
    {
        private readonly Beneficiary _data;
        public RecordForm(Beneficiary data) { _data = data; InitializeUI(); }

        private void InitializeUI()
        {
            Text = "إدخال/تعديل بيانات المستفيد";
            Width = 850; Height = 700;
            StartPosition = FormStartPosition.CenterParent;

            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(15) };

            AddField(panel, "Code Bénéficiaire", new TextBox { Text = _data.CodeBeneficiaire }, t => _data.CodeBeneficiaire = t.Text);
            AddField(panel, "Nom & Prénom", new TextBox { Text = _data.NomBeneficiaire }, t => _data.NomBeneficiaire = t.Text);
            AddField(panel, "Fraction", new TextBox { Text = _data.Fraction }, t => _data.Fraction = t.Text);
            
            AddCombo(panel, "Wilaya", new[] { "MEDEA", "ALGER", "ORAN", "CONSTANTINE" }, _data.Wilaya, v => _data.Wilaya = v);
            AddCombo(panel, "Daïra", new[] { "TABLAT", "MEDEA CENTRE", "SI MAHDI" }, _data.Daira, v => _data.Daira = v);
            AddCombo(panel, "Commune", new[] { "DEUX BASSINS", "TABLAT", "SI MAHDI" }, _data.Commune, v => _data.Commune = v);

            AddField(panel, "N° Décision", new TextBox { Text = _data.DecisionNumero }, t => _data.DecisionNumero = t.Text);
            AddDate(panel, "Date Décision", _data.DecisionDate, d => _data.DecisionDate = d);
            AddNumeric(panel, "Montant (DA)", _data.Montant, n => _data.Montant = n);

            var r1 = new RadioButton { Text = "1ère Tranche", Checked = _data.IsFirstTranche };
            var r2 = new RadioButton { Text = "2ème Tranche", Checked = !_data.IsFirstTranche };
            r2.CheckedChanged += (_, _) => { _data.IsFirstTranche = !r2.Checked; r1.Checked = !r2.Checked; };
            r1.CheckedChanged += (_, _) => { _data.IsFirstTranche = r1.Checked; };
            var pTranche = new Panel { Width = 250, Controls = { r1, r2 }, Margin = new Padding(0,5,0,10) };
            panel.Controls.Add(new Label { Text = "Tranche:", Width = 120 }); panel.Controls.Add(pTranche);

            AddCombo(panel, "Banque", new[] { "BADR", "BNA", "CPA", "BEA", "AGB" }, _data.Banque, v => _data.Banque = v);
            AddField(panel, "N° Compte", new TextBox { Text = _data.CompteNumero }, t => _data.CompteNumero = t.Text);
            AddField(panel, "N° Permis", new TextBox { Text = _data.PermisNumero }, t => _data.PermisNumero = t.Text);
            AddDate(panel, "Date Permis", _data.PermisDate, d => _data.PermisDate = d);

            AddField(panel, "Avancement Plateforme", new TextBox { Text = _data.AvancementPlateforme }, t => _data.AvancementPlateforme = t.Text);
            AddField(panel, "Avancement Poteaux", new TextBox { Text = _data.AvancementPoteaux }, t => _data.AvancementPoteaux = t.Text);
            AddField(panel, "Observations", new TextBox { Multiline = true, Height = 60, Text = _data.Observations }, t => _data.Observations = t.Text);
            AddField(panel, "Nom Responsable", new TextBox { Text = _data.NomResponsable }, t => _data.NomResponsable = t.Text);
            AddField(panel, "Qualité", new TextBox { Text = _data.QualiteResponsable }, t => _data.QualiteResponsable = t.Text);

            var btn = new Button { Text = "💾 حفظ", Dock = DockStyle.Bottom, Height = 40 };
            btn.Click += (_, _) => { DbHelper.Save(_data); DialogResult = DialogResult.OK; };
            Controls.Add(panel);
            Controls.Add(btn);
        }

        private void AddField(Panel p, string label, TextBox tb, Action<TextBox> bind)
        {
            var lbl = new Label { Text = label, Width = 150, Margin = new Padding(0, 5, 0, 2) };
            tb.Width = 300; tb.Margin = new Padding(0, 0, 0, 10);
            p.Controls.AddRange(new Control[] { lbl, tb });
        }
        private void AddCombo(Panel p, string label, string[] items, string sel, Action<string> bind)
        {
            var lbl = new Label { Text = label, Width = 150, Margin = new Padding(0, 5, 0, 2) };
            var cb = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 0, 0, 10) };
            cb.Items.AddRange(items); if (cb.Items.Contains(sel)) cb.SelectedItem = sel;
            cb.SelectedIndexChanged += (_, _) => bind(cb.SelectedItem?.ToString() ?? "");
            p.Controls.AddRange(new Control[] { lbl, cb });
        }
        private void AddDate(Panel p, string label, DateTime val, Action<DateTime> bind)
        {
            var lbl = new Label { Text = label, Width = 150, Margin = new Padding(0, 5, 0, 2) };
            var dt = new DateTimePicker { Width = 300, Value = val, Margin = new Padding(0, 0, 0, 10), Format = DateTimePickerFormat.Short };
            dt.ValueChanged += (_, _) => bind(dt.Value);
            p.Controls.AddRange(new Control[] { lbl, dt });
        }
        private void AddNumeric(Panel p, string label, decimal val, Action<decimal> bind)
        {
            var lbl = new Label { Text = label, Width = 150, Margin = new Padding(0, 5, 0, 2) };
            var num = new NumericUpDown { Width = 300, Value = val, DecimalPlaces = 2, ThousandsSeparator = true, Margin = new Padding(0, 0, 0, 10) };
            num.ValueChanged += (_, _) => bind(num.Value);
            p.Controls.AddRange(new Control[] { lbl, num });
        }
    }
}