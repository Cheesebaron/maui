using Microsoft.Maui.Controls.Platform;
using EColor = ElmSharp.Color;
using EProgressBar = ElmSharp.ProgressBar;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class ActivityIndicatorRenderer : ViewRenderer<ActivityIndicator, EProgressBar>
	{
		static readonly EColor s_defaultColor = ThemeConstants.ProgressBar.ColorClass.Default;

		public ActivityIndicatorRenderer()
		{
			RegisterPropertyHandler(ActivityIndicator.ColorProperty, UpdateColor);
			RegisterPropertyHandler(ActivityIndicator.IsRunningProperty, UpdateIsRunning);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			if (Control == null)
			{
				SetNativeControl(new EProgressBar(Forms.NativeParent)
				{
					IsPulseMode = true,
				}
				.SetSmallStyle());
			}
			base.OnElementChanged(e);
		}

		void UpdateColor(bool initialize)
		{
			if (initialize && Element.Color.IsDefault())
				return;

			Control.Color = (Element.Color.IsDefault()) ? s_defaultColor : Element.Color.ToNative();
		}

		void UpdateIsRunning()
		{
			if (Element.IsRunning && Element.IsEnabled)
			{
				Control.PlayPulse();
			}
			else
			{
				Control.StopPulse();
			}
		}

		protected override void UpdateIsEnabled(bool initialize)
		{
			base.UpdateIsEnabled(initialize);
			UpdateIsRunning();
		}

	};
}