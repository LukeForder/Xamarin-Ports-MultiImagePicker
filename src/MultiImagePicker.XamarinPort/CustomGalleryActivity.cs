using Java.IO;
using System.Collections;

using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using OS = Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;

using Com.Nostra13.Universalimageloader.Cache.Disc.Impl;
using Com.Nostra13.Universalimageloader.Cache.Memory.Impl;
using Com.Nostra13.Universalimageloader.Core;
using Com.Nostra13.Universalimageloader.Core.Listener;
using Com.Nostra13.Universalimageloader.Utils;
using System;
using System.Threading.Tasks;
using System.Linq;
using Java.Util;
using System.Collections.Generic;
using Android.OS;
using Com.Nostra13.Universalimageloader.Core.Assist;

namespace Com.Luminous.Pick
{
    [IntentFilter(
        new[] { Action.ActionPick, Action.ActionPickMultiple },
        Categories = new[] { Intent.CategoryDefault })]
    [Activity]
    public class CustomGalleryActivity : Activity
    {

        GridView gridGallery;
        Handler handler;
        GalleryAdapter adapter;

        ImageView imgNoMedia;
        Button btnGalleryOk;

        string action;
        private ImageLoader imageLoader;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.gallery);

            action = Intent.Action;
            if (action == null)
            {
                Finish();
            }
            InitImageLoader();
            Init();
        }

        private void InitImageLoader()
        {
            try
            {
                string CACHE_DIR = OS.Environment.ExternalStorageDirectory.AbsoluteFile + "/.temp_tmp";
                new File(CACHE_DIR).Mkdirs();

                File cacheDir = StorageUtils.GetOwnCacheDirectory(BaseContext, CACHE_DIR);

                DisplayImageOptions defaultOptions = 
                    new DisplayImageOptions.
					Builder().
					CacheOnDisc(true).
                    ImageScaleType(ImageScaleType.Exactly).
					BitmapConfig(Bitmap.Config.Rgb565).
					Build();

                ImageLoaderConfiguration.Builder builder =
                    new ImageLoaderConfiguration.
					Builder(BaseContext).
					DefaultDisplayImageOptions(defaultOptions).
					DiscCache(new UnlimitedDiscCache(cacheDir)).
					MemoryCache(new WeakMemoryCache());

                ImageLoaderConfiguration config = builder.Build();
                imageLoader = ImageLoader.Instance;
                imageLoader.Init(config);

            }
            catch (Exception e)
            {
                // not going to swallow the exception
                throw;
            }
        }

        private void Init()
        {

            handler = new OS.Handler();

            gridGallery = (GridView)FindViewById(Resource.Id.gridGallery);
            gridGallery.FastScrollEnabled = true;		

            PauseOnScrollListener listener = new PauseOnScrollListener(imageLoader, true, true);
            gridGallery.SetOnScrollListener(listener);

            adapter = new GalleryAdapter(ApplicationContext, imageLoader);

            if (string.Compare(action, Action.ActionPickMultiple, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                FindViewById(Resource.Id.llBottomContainer).Visibility = ViewStates.Visible;
                adapter.IsMultiplePick = true;

            }
            else if (string.Compare(action, Action.ActionPick, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                FindViewById(Resource.Id.llBottomContainer).Visibility = ViewStates.Gone;
                adapter.IsMultiplePick = false;
            }


            gridGallery.Adapter = adapter;
            imgNoMedia = (ImageView)FindViewById(Resource.Id.imgNoMedia);

            gridGallery.ItemClick += OnItemClicked;

            btnGalleryOk = (Button)FindViewById(Resource.Id.btnGalleryOk);
            btnGalleryOk.Click += OnOkClicked;

            Task.Run(() =>
                {
                    handler.Post(() =>
                        {
                            adapter.AddAll(GalleryPhotos);
                            CheckImageStatus();
                        });
                });
        }

        private void CheckImageStatus()
        {
            imgNoMedia.Visibility = (adapter.IsEmpty) ? ViewStates.Visible : ViewStates.Gone;
        }

        /// <summary>
        /// Handles the click event for the 'OK' button, rather than using a Listener
        /// </summary>
        private void OnOkClicked(object sender, EventArgs args)
        {
            string[] allPath = 
                adapter.
				Selected.
				Select(x => x.SdCardPath).
				ToArray();

            Intent data = new Intent().PutExtra("all_path", allPath);
            SetResult(Result.Ok, data);
            Finish();
        }

        /// <summary>
        /// Handles the click event for a photo on the gallery
        /// </summary>
        private void OnItemClicked(object sender, AdapterView.ItemClickEventArgs args)
        {
            if (adapter.IsMultiplePick)
            {
                adapter.ChangeSelection(args.View, args.Position);
            }
            else
            {
                CustomGallery item = adapter[args.Position];
                Intent data = new Intent().PutExtra("single_path", item.SdCardPath);
                SetResult(Result.Ok, data);
                Finish();
            }
        }

        private IEnumerable<CustomGallery> GalleryPhotos
        {
            get
            {
                List<CustomGallery> galleryList = new List<CustomGallery>();

                try
                {
                    string[] columns = 
                        new string[]
                        { 
                            MediaStore.Images.ImageColumns.Data, 
                            MediaStore.Images.ImageColumns.Id 
                        };

                    string orderBy = MediaStore.Images.ImageColumns.Id;

                    ICursor imagecursor = ManagedQuery(
                                              MediaStore.Images.Media.ExternalContentUri,
                                              columns,
                                              null,
                                              null, 
                                              orderBy);

                    if (imagecursor != null && imagecursor.Count > 0)
                    {

                        while (imagecursor.MoveToNext())
                        {
                            CustomGallery item = new CustomGallery();

                            int dataColumnIndex = imagecursor.GetColumnIndex(MediaStore.Images.ImageColumns.Data);

                            item.SdCardPath = imagecursor.GetString(dataColumnIndex);

                            galleryList.Add(item);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw;
                }

                // show newest photo at beginning of the list
                galleryList.Reverse();

                return galleryList;
            }
        }
    }

}