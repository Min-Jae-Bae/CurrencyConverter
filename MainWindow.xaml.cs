using Newtonsoft.Json;
using System;
using System.Data;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Diagnostics;


namespace CurrencyConverter
{

    public partial class MainWindow : Window
    {
        Root val = new Root();

        //API 받을 최상위 클래스
        public class Root
        {
            public Rate rates { get; set; }
            public long timestamp;
            public string license;
        }

        public class Rate
        {
            public double USD { get; set; }
            public double KRW { get; set; }
            public double JPY { get; set; }
            public double CNY { get; set; }
            public double EUR { get; set; }
        }

        //통화 화폐를 받을 클래스

        public MainWindow()
        {
            InitializeComponent();
            GetValue();
        }

        //데이터를 가져오기
        private async void GetValue()
        {
            val = await GetData<Root>("https://openexchangerates.org/api/latest.json?app_id=7eedeb84978e4d1ca58cf77b822d4247");
            BindCurrency();
        }

        public static async Task<Root> GetData<T>(string url)
        {
            var myRoot = new Root();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var ResponseString = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine("API 응답: " + ResponseString);
                        var ResponseObject = JsonConvert.DeserializeObject<Root>(ResponseString);
                        return ResponseObject;
                    }
                    return myRoot;
                }
            }
            catch (Exception)
            {

                return myRoot;
            }
        }

        //변환 클릭
        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            double ConvertedValue;
            //만약에 텍스트가 들어가 있지 않거나 공백일 경우
            if (txtCurrency.Text == null || txtCurrency.Text.Trim() == "")
            {
                //메시지 박스를 보여주고 입력하지 않았다고 창을 띄운다.
                MessageBox.Show("통화를 입력해주세요", "정보", MessageBoxButton.OK, MessageBoxImage.Information);
                //환율에 포커스 둔다.
                txtCurrency.Focus();
                return;
            }
            else if (cmbFromCurrency.SelectedValue == null || cmbFromCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("변환하고 싶은 통화를 선택해주세요", "정보", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbFromCurrency.Focus();
                return;
            }
            else if (cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("변환할 통화를 선택해주세요", "정보", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbToCurrency.Focus();
                return;
            }

            //환율을 똑같이 적용했을 때
            //환율 그대로 출력한다.
            if (cmbFromCurrency.Text == cmbToCurrency.Text)
            {
                //넣은 값을 double로 변경한다.
                //출력한다.
                ConvertedValue = double.Parse(txtCurrency.Text);
                lblCurrency.Content = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");
            }
            else
            {
                //환율을 다르게 적용했을 때
                //대입 가격 * 환율 / 변경 환율로 보여준다.
                ConvertedValue = double.Parse(txtCurrency.Text) *
                    double.Parse(cmbToCurrency.SelectedValue.ToString()) /
                    double.Parse(cmbFromCurrency.SelectedValue.ToString());

                lblCurrency.Content = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");

            }

        }
        //지우기 클릭
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
        }

        #region Bind Currency From and To Combobox
        private void BindCurrency()
        {
            //데이터 테이블을 만든다.
            DataTable dt = new DataTable();
            //컬럼 값을 지정한다.
            dt.Columns.Add("Text");
            dt.Columns.Add("Value");
            //row 값을 지정한다.
            dt.Rows.Add("--선택하기--", 0);
            dt.Rows.Add("USD", val.rates.USD);
            dt.Rows.Add("KRW", val.rates.KRW);
            dt.Rows.Add("JPY", val.rates.JPY);
            dt.Rows.Add("CNY", val.rates.CNY);
            dt.Rows.Add("EUR", val.rates.EUR);

            //기본값을 처음으로 지정하기
            cmbFromCurrency.ItemsSource = dt.DefaultView;
            //멤버랑, 지정값을 기본값으로 놓기
            cmbFromCurrency.DisplayMemberPath = "Text";
            cmbFromCurrency.SelectedValuePath = "Value";
            cmbFromCurrency.SelectedIndex = 0;

            cmbToCurrency.ItemsSource = dt.DefaultView;
            //멤버랑, 지정값을 기본값으로 놓기
            cmbToCurrency.DisplayMemberPath = "Text";
            cmbToCurrency.SelectedValuePath = "Value";
            cmbToCurrency.SelectedIndex = 0;
        }
        #endregion
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ClearControl()
        {
            //모든걸 처음으로 초기화하기
            txtCurrency.Text = string.Empty;
            //안에 있는 아이템들이 0보다 클때 모두다 처음으로 돌린다
            if (cmbFromCurrency.Items.Count > 0)
                cmbFromCurrency.SelectedIndex = 0;
            if (cmbToCurrency.Items.Count > 0)
                cmbToCurrency.SelectedIndex = 0;
            lblCurrency.Content = "";
            txtCurrency.Focus();
        }
    }

}
