using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Content;
using System;
using System.Collections.Generic;
using Android.Views;
using System.Text;
using System.Globalization;
using Android.Runtime;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using Android.Support.Design.Widget;
using Android.Views.InputMethods;
using Android.Graphics;

namespace RRTechAssessment
{
    [Activity(Label = "RR Tech Assessment", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        private TextView txtResults;
   
        private TextView txtTitleUp;
        private EditText txtVal;
        private string filePath;
        private Button BtnOpen;
        private Button btnSearch;
        private Button btnClear;
        public const int FilechooserResultcode = 1;
        public const int FILE_SELECT_CODE = 0;
        private string fileText = "";
        private RelativeLayout mLayout;

        public static readonly string[] ExtensionAllowed = { ".txt" };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);

            mLayout = FindViewById<RelativeLayout>(Resource.Id.mainLayout);

            SetSupportActionBar(toolbar);

            SupportActionBar.Title = "RR Assessement";

            BtnOpen = FindViewById<Button>(Resource.Id.btnOpenFile);

            btnClear = FindViewById<Button>(Resource.Id.btnClearAll);

            BtnOpen.Click += BtnOpen_Click;

            btnSearch = FindViewById<Button>(Resource.Id.btnSearch);

            btnSearch.Click += BtnSearch_Click;

            btnSearch.Visibility = ViewStates.Invisible;

            btnClear.Click += BtnClear_Click;

            btnClear.Visibility = ViewStates.Gone;

            txtTitleUp = FindViewById<TextView>(Resource.Id.txtTitle);

            txtVal = FindViewById<EditText>(Resource.Id.txtValue);

            txtVal.TextChanged += TxtVal_TextChanged;

            txtVal.Visibility = ViewStates.Invisible;

            txtResults = FindViewById<TextView>(Resource.Id.txtResult);
      
            txtTitleUp.Visibility = ViewStates.Invisible;

            txtResults.Visibility = ViewStates.Invisible;
 
            //Few tweaks
            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);


        }

        private void TxtVal_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (txtVal.Text != "")
            {
                btnSearch.Visibility = ViewStates.Visible;
            }
            else
            {
                btnSearch.Visibility = ViewStates.Invisible;
            }
        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {

            InputMethodManager manager = (InputMethodManager)GetSystemService(InputMethodService);
            manager.HideSoftInputFromWindow(txtVal.WindowToken, 0);

            SaveToString();

            string strInput = fileText;
            strInput = strInput.Replace(",", ""); //cleaning up

            string[] arr = strInput.Split(' '); //Array

            //Just ensuring I have the right file //**Self Verification
            Toast.MakeText(this, "File Length: " + fileText.Length.ToString(), ToastLength.Short).Show();

            ProgressDialog FrProgressCalc = new ProgressDialog(this);

            FrProgressCalc.SetTitle("Searching");

            FrProgressCalc.SetMessage("Getting your results");

            FrProgressCalc.SetCanceledOnTouchOutside(false);

            FrProgressCalc.Show();

            await Task.Delay(1000);

            txtTitleUp.Visibility = ViewStates.Visible;

            searchSimilar(arr);

            await Task.Delay(1000);

            FrProgressCalc.Dismiss();

            btnClear.Visibility = ViewStates.Visible;
        }

        private void clear()
        {

            txtTitleUp.Visibility = ViewStates.Invisible;
            txtVal.Text = "";
            //txtVal.Visibility = ViewStates.Invisible;
            //btnSearch.Visibility = ViewStates.Invisible;
            txtResults.Text = "";
            btnClear.Visibility = ViewStates.Gone;
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            clear();
        }

        public static String getPath(Context context, Android.Net.Uri uri) //throws URISyntaxException
        {

            var mediaStoreImagesMediaData = "_data";
            string[] projection = { mediaStoreImagesMediaData };
            Android.Database.ICursor cursor = Application.Context.ContentResolver.Query(uri, projection, null, null, null);
            int columnIndex = cursor.GetColumnIndexOrThrow(mediaStoreImagesMediaData);
            cursor.MoveToFirst();
            return cursor.GetString(columnIndex);

        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedStr = text.Normalize(NormalizationForm.FormD);
            var strBuilder = new StringBuilder();

            foreach (var c in normalizedStr)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    strBuilder.Append(c);
                }
            }

            return strBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {

            switch (requestCode)
            {
                case FILE_SELECT_CODE:
                    if (resultCode == Result.Ok)
                    {
                        // Get the Uri of the selected file 
                        Android.Net.Uri uri = data.Data;

                        // Get the path
                        filePath = getPath(this, uri);

                        var extension = System.IO.Path.GetExtension(filePath);

                        var filename = System.IO.Path.GetFileName(filePath);

                        //Checking if text File
                        if (extension == ".txt")
                        {
                            txtVal.Visibility = ViewStates.Visible;

                            Toast.MakeText(this, filename + " has been chosen", ToastLength.Long).Show();

                        }
                        else
                        {

   
                            Snackbar
                                 .Make(mLayout, "Please select a text File (.txt)", Snackbar.LengthLong)
                                 .SetAction("Ok", (view) => { })
                                  .Show(); // Show

                            btnSearch.Enabled = false;
                            //Toast.MakeText(this, "", ToastLength.Long).Show();
                        }

                        
                    }
                    break;
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        private async void searchSimilar(string[] strArr)
        {
            int count = 0;
            string tempStr = "";

            await Task.Delay(1000);

            for (int i = 0; i<strArr.Length;i++)
            {
                tempStr = strArr[i];
                if (tempStr.Equals(txtVal.Text, StringComparison.OrdinalIgnoreCase)) //Will just ignore the case to normalize the search
                    count++;
            }

            await Task.Delay(1000);

            if (count > 0)
            {
                var word = txtVal.Text.ToUpper();
                txtResults.SetTextColor(Color.ParseColor("#000000"));
                txtResults.Text = word + " has been found " + count + " Times";
               
            }
            else
            {
                txtResults.SetTextColor(Color.ParseColor("#9b0005"));
                txtResults.Text = txtVal.Text + " has not been found in the text file";
            }

           

            txtResults.Visibility = Android.Views.ViewStates.Visible;

           

            

        }


        private void SaveToString()
        {
            try
            {

                StreamReader sr = new StreamReader(filePath);
                fileText = sr.ReadToEnd();

            }
            catch (Exception e)
            {
                String error = "";
                error = e.Message;

                Toast.MakeText(this, "Could Not read the File : " + error, ToastLength.Long).Show(); // Error Message
            }

        }

        private void BtnOpen_Click(object sender, System.EventArgs e)
        {
            //Call the file Dialog Handler

            Intent intent = new Intent(Intent.ActionGetContent);
            intent.SetType("*/*");
            intent.AddCategory(Intent.CategoryOpenable);

            try
            {
                StartActivityForResult(
                        Intent.CreateChooser(intent, "Select a File"),
                        FILE_SELECT_CODE);
            }
            catch (Android.Content.ActivityNotFoundException ex)
            {
                // Potentially direct the user to the Market with a Dialog
                Toast.MakeText(this, "Please install a File Manager.", ToastLength.Long).Show();
            }


        }
    }




}

