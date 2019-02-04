﻿using System;
using Captura.Base.Services;
using Xunit;

namespace Tests
{
    [Collection(nameof(Tests))]
    public class WindowTests
    {
        [Fact]
        public void ZeroPointerWindow()
        {
            var platformServices = ServiceProvider.Get<IPlatformServices>();

            Assert.Throws<ArgumentException>(() => platformServices.GetWindow(IntPtr.Zero));
        }

        [Fact]
        public void DesktopWindowNotZero()
        {
            var platformServices = ServiceProvider.Get<IPlatformServices>();

            Assert.NotEqual(IntPtr.Zero, platformServices.DesktopWindow.Handle);
        }
    }
}
