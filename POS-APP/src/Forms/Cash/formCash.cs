﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace POS_APP {
    public partial class formCash : Form {
        double totalPrice;
        bool canBuy = false;
        string pay = "Pay by Cash";

        public formCash() {
            InitializeComponent();
        }

        private void formCash_Load(object sender, EventArgs e) {
            showTotalDetail();
        }

        private void showTotalDetail() {
            lblTotalAmount.Text = productData.proName.Count.ToString("#,#");

            totalPrice = 0;
            for (int i = 0; i < productData.proName.Count; i++) {
                totalPrice += productData.amount[i] * productData.proPrice[i];
            }
            productData.totalPrice = totalPrice;
            lblTotalPrice.Text = totalPrice.ToString("#,#.00");
        }

        private void btnBack_Click(object sender, EventArgs e) {
            var form = new formPayment();
            form.Show();
            this.Hide();
        }

        private void btnCalcTotal_Click(object sender, EventArgs e) {
            double amount = Convert.ToDouble(txtAmount.Text);

            double change = amount - totalPrice;

            // change cannot negative number
            if (change >= 0) {
                txtChange.Text = change.ToString("#,#.00");
                canBuy = true;
            }
            else {
                MessageBox.Show(
                    "Money not enough!",
                    "Warning"
                );
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e) {
            if (canBuy) {
                putProductDB();
                postReq();

                // Clear
                memberData.clearData();
                productData.clearData();

                MessageBox.Show("Purchase success", "Notification");

                // Go to Home form
                var form = new formHome();
                form.Show();
                this.Hide();
            }
        }

        private void putProductDB() {
            dbConfig.connection.Open();
            try {
                for (int i = 0; i < productData.proName.Count; i++) {
                    var adapter = new SqlDataAdapter();

                    string sql =
                        $"UPDATE Products " +
                        $"SET amount = '{productData.proAmount[i]}' " +
                        $"WHERE product_id = '{productData.proID[i]}'";
                    adapter.UpdateCommand = dbConfig.connection.CreateCommand();
                    adapter.UpdateCommand.CommandText = sql;
                    adapter.UpdateCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            dbConfig.connection.Close();
        }

        private void postReq() {
            // Date now when payment
            DateTime dt = new DateTime();
            dt = DateTime.Now;
            string dateNow = dt.ToString().Split()[0];

            // Check used memeber with phone
            bool member = memberData.id_pur != null;
            string member_col = (member) ? "member_id," : "";
            string member_id = (member) ? $"'{memberData.id_pur}'," : "";

            dbConfig.connection.Open();
           
            try {
                for (int i = 0; i < productData.proName.Count; i++) {
                    var adapter = new SqlDataAdapter();
                    string sql =
                        $"INSERT INTO RequestProduct " +
                        $"(product_id," +
                        $"req_amount," +
                        $"{member_col}" +
                        $"req_price," +
                        $"req_date," +
                        $"req_status," +
                        $"payment_method, " +
                        $"payment_status, " +
                        $"payment_date) " +
                        $"VALUES(" +
                        $"'{productData.proID[i]}'," +
                        $"'{productData.amount[i]}'," +
                        $"{member_id}" +
                        $"'{productData.proPrice[i] * productData.amount[i]}'," +
                        $"'{dateNow}'," +
                        $"'YES'," +
                        $"'{pay}'," +
                        $"'YES'," +
                        $"'{dateNow}'" +
                        $")";

                    adapter.InsertCommand = dbConfig.connection.CreateCommand();
                    adapter.InsertCommand.CommandText = sql;
                    adapter.InsertCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            dbConfig.connection.Close();
        }

    }
}