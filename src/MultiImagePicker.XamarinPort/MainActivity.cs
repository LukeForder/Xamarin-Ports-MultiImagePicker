using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Com.Nostra13.Universalimageloader.Core;
using Android.Views;
using Com.Nostra13.Universalimageloader.Core.Assist;
using Android.Graphics;
using Android.Content;
using System.Collections.Generic;
using Com.Nostra13.Universalimageloader.Cache.Memory.Impl;

namespace  Com.Luminous.Pick
{
    [Activity(MainLauncher = true)]
    public class MainActivity : Activity
    {

        GridView gridGallery;
        Handler handler;
        GalleryAdapter adapter;

        ImageView imgSinglePick;
        Button btnGalleryPick;
        Button btnGalleryPickMul;

        ViewSwitcher viewSwitcher;
        ImageLoader imageLoader;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.main);

            InitImageLoader();
            Init();
        }

        private void InitImageLoader()
        {
            DisplayImageOptions defaultOptions = 
                new DisplayImageOptions.
                Builder().
                CacheOnDisc().
                ImageScaleType(ImageScaleType.ExactlyStretched).
                BitmapConfig(Bitmap.Config.Rgb565).
                Build();

            ImageLoaderConfiguration.Builder builder = 
                new ImageLoaderConfiguration.
                Builder(this).
                DefaultDisplayImageOptions(defaultOptions).
                MemoryCache(new WeakMemoryCache());

            ImageLoaderConfiguration config = builder.Build();
            imageLoader = ImageLoader.Instance;
            imageLoader.Init(config);
        }

        private void Init()
        {

            handler = new Handler();
            gridGallery = FindViewById<GridView>(Resource.Id.gridGallery);
            gridGallery.FastScrollEnabled = true;

            adapter = new GalleryAdapter(ApplicationContext, imageLoader);
            adapter.IsMultiplePick = false;
            gridGallery.Adapter = adapter;

            viewSwitcher = FindViewById<ViewSwitcher>(Resource.Id.viewSwitcher);
            viewSwitcher.DisplayedChild = 1;

            imgSinglePick = FindViewById<ImageView>(Resource.Id.imgSinglePick);

            btnGalleryPick = FindViewById<Button>(Resource.Id.btnGalleryPick);

            btnGalleryPick.Click += OnPickOneClicked;

            btnGalleryPickMul = FindViewById<Button>(Resource.Id.btnGalleryPickMul);

            btnGalleryPickMul.Click += OnPickManyClicked;

        }

        private void OnPickOneClicked(object sender, EventArgs args)
        {
            Intent i = new Intent(Action.ActionPick);
            StartActivityForResult(i, 100);
        }

        private void OnPickManyClicked(object sender, EventArgs args)
        {
            Intent i = new Intent(Action.ActionPickMultiple);
            StartActivityForResult(i, 200);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 100 && resultCode == Result.Ok)
            {
                adapter.Clear();

                viewSwitcher.DisplayedChild = 1;
                string single_path = data.GetStringExtra("single_path");

                imageLoader.DisplayImage("file://" + single_path, imgSinglePick);

            }
            else if (requestCode == 200 && resultCode == Result.Ok)
            {
                String[] all_path = data.GetStringArrayExtra("all_path");

                List<CustomGallery> dataT = new List<CustomGallery>();

                foreach (string uri in all_path)
                {
                    CustomGallery item = new CustomGallery();
                    item.SdCardPath = uri;

                    dataT.Add(item);
                }

                viewSwitcher.DisplayedChild = 0;

                adapter.AddAll(dataT);
            }
        }

    }

}

