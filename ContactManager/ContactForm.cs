using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ContactManager.Models;
using System.Text.RegularExpressions;

namespace ContactManager
{
    public partial class ContactForm : Form
    {
        private ContactManagerDBEntities _entities = new ContactManagerDBEntities();

        public ContactForm()
        {
            InitializeComponent();

            InitControls();

            IndexView();
        }

        private void InitControls()
        {
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void IndexView()
        {
            dataGridView1.DataSource = _entities.Contacts.ToList();

            tabControl1.TabPages[1].Text = "Add";
            tabControl1.SelectedTab = tabPage1;

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage1)
            {
                AddView();
            }
        }

        private void AddView()
        {
            tabControl1.TabPages[1].Text = "Add";
            button_Save.Tag = null;
            textBox_FirstName.Text = "";
            textBox_LastName.Text = "";
            textBox_Phone.Text = "";
            textBox_Email.Text = "";
        }

        private void EditView(Contact contactToEdit)
        {
            tabControl1.TabPages[1].Text = "Edit";
            button_Save.Tag = contactToEdit.Id;
            textBox_FirstName.Text = contactToEdit.FirstName;
            textBox_LastName.Text = contactToEdit.LastName;
            textBox_Phone.Text = contactToEdit.Phone;
            textBox_Email.Text = contactToEdit.Email;
            tabControl1.SelectedTab = tabPage2;
        }

        private void dataGridView1_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //Tao menu ngu canh
                ContextMenuStrip menu = new ContextMenuStrip();
                menu.Items.Add("Edit", null, new EventHandler(Edit_Click));
                menu.Items.Add("Delete", null, new EventHandler(Delete_Click));
                //Di chuyen den dong hien hanh
                dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                //hien thi menu
                Point pt = dataGridView1.PointToClient(Control.MousePosition);
                menu.Show(dataGridView1, pt.X, pt.Y);
            }
        }

        private void Edit_Click(object sender, EventArgs args)
        {
            int id = (int)dataGridView1.SelectedCells[0].Value;
            var contactToEdit = (from c in _entities.Contacts
                                 where c.Id == id
                                 select c).FirstOrDefault();
            EditView(contactToEdit);
        }

        private void Delete_Click(object sender, EventArgs args)
        {
            int id = (int)dataGridView1.SelectedCells[0].Value;
            var contactToDelete = (from c in _entities.Contacts
                                   where c.Id == id
                                   select c).FirstOrDefault();
            if (MessageBox.Show(string.Format("Do you want to delete {0} {1} ?", contactToDelete.FirstName,
                 contactToDelete.LastName), "Delete contact", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                _entities.DeleteObject(contactToDelete);
                _entities.SaveChanges();
                IndexView();
            }
        }

        private void button_Save_Click(object sender, EventArgs e)
        {
            Contact contactToEdit = GetContactToCreate();
            if (ValidateContact(contactToEdit))
            {
                if (contactToEdit.Id > 0) //Contact da ton tai

                {
                    var originalContact = (from c in _entities.Contacts
                                           where c.Id == contactToEdit.Id
                                           select c).FirstOrDefault();
                    _entities.ApplyCurrentValues(originalContact.EntityKey.EntitySetName, contactToEdit);
                }
                else //Contact moi
                {
                    contactToEdit.Id = _entities.Contacts .Count ()> 0 ? _entities.Contacts.Max(c => c.Id) +1 : 1;
                    _entities.AddToContacts(contactToEdit);
                }
                _entities.SaveChanges();
                IndexView();
            }
        }

        private bool ValidateContact(Contact contactToValidate)
        {
            bool isValid = true;
            errorProvider1.Clear();

            if (contactToValidate.FirstName.Trim().Length == 0)
            {
                errorProvider1.SetError(textBox_FirstName, "First name is required.");
                isValid = false;
            }
            if (contactToValidate.LastName.Trim().Length == 0)
            {
                errorProvider1.SetError(textBox_LastName, "Last name is required.");
                isValid = false;
            }
            if (contactToValidate.Phone.Length > 0 && !Regex.IsMatch(contactToValidate.Phone, @"((\(\d{3}\) ?)|(\d{3}-))?\d{3}-\d{4}"))
            {
                errorProvider1.SetError(textBox_Phone, "Invalid phone number.");
                isValid = false;
            }
            if (contactToValidate.Email.Length > 0 && !Regex.IsMatch(contactToValidate.Email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
            {
                errorProvider1.SetError(textBox_Email, "Invalid email address.");
                isValid = false;
            }
            return isValid;
        }

        private Contact GetContactToCreate()
        {
            return new Contact()
            {
                Id = button_Save.Tag == null ? 0 : (int)button_Save.Tag,
                FirstName = textBox_FirstName.Text.Trim(),
                LastName = textBox_LastName.Text.Trim(),
                Phone = textBox_Phone.Text.Trim(),
                Email = textBox_Email.Text.Trim()
            };
        }

    }
}
