﻿using System;
using System.Drawing;
using System.Windows;
using Captura.Base.Services;
using Captura.Base.Video;
using Captura.ViewModels;
using Captura.Windows;
using Screna;

namespace Captura.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RegionSelectorProvider : IRegionProvider
    {
        private readonly Lazy<RegionSelector> _regionSelector;
        private readonly RegionItem _regionItem;
        private readonly RegionSelectorViewModel _viewModel;

        public RegionSelectorProvider(IVideoSourcePicker videoSourcePicker, RegionSelectorViewModel viewModel)
        {
            _viewModel = viewModel;

            _regionSelector = new Lazy<RegionSelector>(() =>
            {
                var reg = new RegionSelector(videoSourcePicker, viewModel);

                reg.SelectorHidden += () => SelectorHidden?.Invoke();

                return reg;
            });

            _regionItem = new RegionItem(this);
        }

        public bool SelectorVisible
        {
            get => _regionSelector.Value.Visibility == Visibility.Visible;
            set
            {
                if (value)
                    _regionSelector.Value.Show();
                else _regionSelector.Value.Hide();
            }
        }

        public Rectangle SelectedRegion
        {
            get => _viewModel.SelectedRegion;
            set => _viewModel.SelectedRegion = value;
        }

        public IVideoItem VideoSource => _regionItem;

        public void Lock() => _regionSelector.Value.Lock();

        public void Release() => _regionSelector.Value.Release();

        public event Action SelectorHidden;

        public IntPtr Handle => _regionSelector.Value.Handle;
    }
}