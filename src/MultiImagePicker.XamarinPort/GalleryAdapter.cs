using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using Com.Nostra13.Universalimageloader.Core;
using Com.Nostra13.Universalimageloader.Core.Listener;
using System.Linq;

namespace Com.Luminous.Pick
{
    public class GalleryAdapter : BaseAdapter<CustomGallery>
    {

        public class ViewHolder : Java.Lang.Object
        {
            public ImageView ImgQueue
            {
                get;
                set;
            }

            public ImageView ImgQueueMultiSelected
            {
                get;
                set;
            }
        }

        private class SimpleImageLoadingListenerImpl : SimpleImageLoadingListener
        {

            public SimpleImageLoadingListenerImpl(ViewHolder holder)
            {
                this.holder = holder;
            }

            ViewHolder holder;

            public override  void OnLoadingStarted(String imageUri, View view)
            {
                holder.
				ImgQueue.
				SetImageResource(Resource.Drawable.no_media);

                base.OnLoadingStarted(imageUri, view);
            }
        }

        private Context mContext;
        private LayoutInflater inflater;
        private List<CustomGallery> data;
        ImageLoader imageLoader;
        private bool isActionMultiplePick;

        public GalleryAdapter(Context c, ImageLoader imageLoader)
        {

            this.data = new List<CustomGallery>();
            this.inflater = (LayoutInflater)c.GetSystemService(Context.LayoutInflaterService);
            this.mContext = c;
		
            this.imageLoader = imageLoader;
            // clearCache();
        }

        public override int Count
        {
            get
            {

                return data.Count;
            }
        }

        public override CustomGallery this [int index]
        {
            get
            {
                return data[index];
            }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public bool IsMultiplePick
        {
            get
            {
                return isActionMultiplePick;
            }
            set
            {
                this.isActionMultiplePick = value;
            }
        }

	
        public void selectAll(bool selection)
        {
            for (int i = 0; i < data.Count; i++)
            {
                data[i].IsSelected = selection;

            }
            NotifyDataSetChanged();
        }

        public bool AllSelected
        {
            get
            {
                return data.All(x => x.IsSelected);
            }
        }

        public bool AnySelected
        {
            get
            {
                return data.Any(x => x.IsSelected);
            }
        }

        public IEnumerable<CustomGallery> Selected
        {
            get
            {
                return 
					data.
					Where(x => x.IsSelected).
					ToList();
            }
        }

        public void AddAll(IEnumerable<CustomGallery> files)
        {

            try
            {
                this.data.Clear();
                this.data.AddRange(files);

            }
            catch (Exception e)
            {
                throw;
            }

            NotifyDataSetChanged();
        }

        public void ChangeSelection(View v, int position)
        {

            data[position].IsSelected = !data[position].IsSelected;

            ((GalleryAdapter.ViewHolder)v.Tag).ImgQueueMultiSelected.Selected = data[position].IsSelected;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            GalleryAdapter.ViewHolder holder;
	
            if (convertView == null)
            {

                convertView = inflater.Inflate(Resource.Layout.gallery_item, null);

                holder = new GalleryAdapter.ViewHolder();

                holder.ImgQueue = (ImageView)convertView.FindViewById(Resource.Id.imgQueue);

                holder.ImgQueueMultiSelected = (ImageView)convertView.FindViewById(Resource.Id.imgQueueMultiSelected);

                holder.ImgQueueMultiSelected.Visibility = (isActionMultiplePick) ? ViewStates.Visible : ViewStates.Gone;

                convertView.Tag = holder;

            }
            else
            {
                holder = (GalleryAdapter.ViewHolder)convertView.Tag;
            }

            holder.ImgQueue.Tag = position;

            try
            {

                imageLoader.DisplayImage(
                    "file://" + data[position].SdCardPath,
                    holder.ImgQueue,
                    new GalleryAdapter.SimpleImageLoadingListenerImpl(holder));

                if (isActionMultiplePick)
                {

                    holder.ImgQueueMultiSelected.Selected = data[position].IsSelected;
                }

            }
            catch (Exception e)
            {
                throw;
            }

            return convertView;
        }


        public void ClearCache()
        {
            imageLoader.ClearDiscCache();
            imageLoader.ClearMemoryCache();
        }

        public void Clear()
        {
            data.Clear();
            NotifyDataSetChanged();
        }
    }
}
