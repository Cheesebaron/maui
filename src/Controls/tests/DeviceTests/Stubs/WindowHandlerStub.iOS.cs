﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class WindowHandlerStub : ElementHandler<IWindow, UIWindow>, IWindowHandler
	{
		TaskCompletionSource<bool> _finishedDisconnecting = new TaskCompletionSource<bool>();
		public Task FinishedDisconnecting => _finishedDisconnecting.Task;
		IView _currentView;

		public static IPropertyMapper<IWindow, WindowHandlerStub> WindowMapper = new PropertyMapper<IWindow, WindowHandlerStub>(WindowHandler.Mapper)
		{
			[nameof(IWindow.Content)] = MapContent
		};

		void UpdateContent(UIWindow platformView)
		{
			CloseView(_currentView, platformView, () =>
			{
				var view = VirtualView.Content.ToPlatform(MauiContext);
				_currentView = VirtualView.Content;

				if (VirtualView.Content is IFlyoutView)
				{
					var vc =
						(_currentView.Handler as IPlatformViewHandler)
							.ViewController;

					vc.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;

					VirtualView.Created();
					PlatformView.RootViewController.PresentViewController(vc, false, () => VirtualView.Activated());
				}
				else
				{
					VirtualView.Created();
					AssertionExtensions.FindContentView().AddSubview(view);
					VirtualView.Activated();
				}
			});
		}

		void CloseView(IView view, UIWindow platformView, Action finishedClosing)
		{
			if (view == null)
			{
				finishedClosing?.Invoke();
				return;
			}

			var vc = (view.Handler as IPlatformViewHandler).ViewController;
			var virtualView = VirtualView;

			if (view is IFlyoutView)
			{
				var pvc = platformView?.RootViewController?.PresentedViewController;
				// This means shell never got to the point of being preesented
				if (pvc == null)
				{
					finishedClosing?.Invoke();
					return;
				}

				virtualView.Deactivated();
				pvc.DismissViewController(false,
					() =>
					{
						finishedClosing.Invoke();
						virtualView.Destroying();
					});
			}
			else
			{
				view
					.ToPlatform()
					.RemoveFromSuperview();

				finishedClosing.Invoke();

				virtualView.Deactivated();
				virtualView.Destroying();
			}

		}

		public static void MapContent(WindowHandlerStub handler, IWindow window)
		{
			handler.UpdateContent(handler.PlatformView);
		}

		protected override void DisconnectHandler(UIWindow platformView)
		{
			CloseView(VirtualView.Content, platformView, () => _finishedDisconnecting.SetResult(true));
			base.DisconnectHandler(platformView);
		}

		public WindowHandlerStub()
			: base(WindowMapper)
		{
		}

		protected override UIWindow CreatePlatformElement()
		{
			return MauiContext.Services.GetService<UIWindow>();
		}
	}
}
