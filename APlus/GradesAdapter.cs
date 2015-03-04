using Android.Content;
using Android.Views;
using Android.Widget;

namespace APlus
{
    public class CustomAdapter : BaseAdapter
    {
        readonly Context _context;

        public CustomAdapter(Context context)
        {
            _context = context;
        }

        public override int Count
        {
            get { return 2; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return 0;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            TextView textView;

            if (convertView == null) {  // if it's not recycled, initialize some attributes
                textView = new TextView(_context);
                textView.LayoutParameters = new GridView.LayoutParams(85, 85);
                textView.SetPadding(8, 8, 8, 8);
                textView.Text = "Test";
            }
            else textView = (TextView)convertView;

            return textView;
        }
    }
}