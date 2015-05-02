using Android.Content;
using Android.Views;
using Android.Widget;

namespace APlus
{
	public class GradesAdapter : BaseAdapter
	{
		readonly Context _context;
		private string[] _subjectGrades;

		public GradesAdapter (Context context, string[] subjectGrades)
		{
			_context = context;
			_subjectGrades = subjectGrades;
		}

		public override int Count
		{
			get { return _subjectGrades.Length; }
		}

		public override Java.Lang.Object GetItem (int position)
		{
			return null;
		}

		public override long GetItemId (int position)
		{
			return 0;
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			TextView textView;

			if (convertView == null) // if it's not recycled, initialize some attributes
			{  
				textView = new TextView (_context);
				textView.LayoutParameters = new GridView.LayoutParams (ViewGroup.LayoutParams.MatchParent, 85);
				//textView.SetPadding(8, 8, 8, 8);
				textView.Text = _subjectGrades [position];
			} 
			else textView = (TextView)convertView;

			return textView;
		}
	}
}