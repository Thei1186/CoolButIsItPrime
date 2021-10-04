using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoolButIsItPrime {
    public partial class Form1 : Form {
        private readonly SynchronizationContext synchronizationContext;
        private PrimeGenerator primeGenerator;
        private bool sequential = true;
        private bool cancelled = false;
        public Form1() {
            InitializeComponent();
            primeGenerator = new PrimeGenerator();
            synchronizationContext = SynchronizationContext.Current;
            button2.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        private async void button1_Click(object sender, EventArgs e) {
            button1.Enabled = false;
            button2.Enabled = true;
            cancelled = false;
            label2.Text = "";

            listBox1.Items.Clear();
            bool isFromValueANumber = long.TryParse(textBox1.Text, out long fromValue);
            bool isToValueANumber = long.TryParse(textBox2.Text, out long toValue);
            if (!isFromValueANumber || !isToValueANumber) {
                label2.Text = "The inserted values has to be numbers";
            }

            Stopwatch sw = Stopwatch.StartNew();
            List<long> listOfPrimes = new List<long>();
            if (sequential) {
                listOfPrimes = await primeGenerator.GetPrimesSequentialAsync(fromValue, toValue);
            } else {
                listOfPrimes = await primeGenerator.GetPrimesParallelAsync(fromValue, toValue);
            }
            sw.Stop();
            label4.Text = (sw.ElapsedMilliseconds / 1000F).ToString();
            if (!(listOfPrimes.Count > 0)) {
                label2.Text = "No primes found";
            }
            listOfPrimes.Sort();
            await Task.Run(() => {
                List<long> tempList = new List<long>();
                int count = 0;
                int batchSize = 1000;
                int remaining = listOfPrimes.Count;
                for (int i = 0; i < listOfPrimes.Count; i++) {
                    if (cancelled) {
                        break;
                    }
                    count++;
                    remaining--;
                    tempList.Add(listOfPrimes[i]);
                    if (count == batchSize || (remaining - count ) < 0) {
                        count = 0;
                        UpdateUI(tempList);
                        tempList.Clear();
                    }
                }
            });
            //  listBox1.Refresh();
            button2.Enabled = false;
            button1.Enabled = true;
        }
        public void UpdateUI(List<long> value) {
            object[] arr = value.Cast<object>().ToArray();
            //Send the update to our UI thread
            synchronizationContext.Post(new SendOrPostCallback(o => {
                listBox1.Items.AddRange((object[])o);
            }), arr);

            // Let the thread sleep as to avoid freezing the GUI
            Thread.Sleep(35);
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e) {
            RadioButton rb = sender as RadioButton;

            if (rb.Name.Equals("Sequential") && rb.Checked) {
                sequential = true;
            } else {
                sequential = false;
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            cancelled = true;
            button2.Enabled = false;
        }
    }
}
