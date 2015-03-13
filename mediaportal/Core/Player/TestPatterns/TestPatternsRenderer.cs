using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace MediaPortal.Player.TestPatterns
{
	public enum TestPatternApect { f16by9, f21by9 }

	public class TestPatternsRenderer
	{
		Texture[] _texture = new Texture[2];

		TestPatternApect _tpAspect;
		int _testPatternIndex;

		int _countToggle = 0;
		int _textureIndex = 0;

		private String[] renderModeHalfText = { "", "Left Image", "Right Image", "Top Image", "Bottom Image" };

		TestPatternList _testPatterns = new TestPatternList();

		public TestPatternsRenderer()
		{
			_testPatterns.Load();
		}

		private Texture LoadTexture(String file, int width, int height)
		{
			Stream stm = new FileStream(file, FileMode.Open);

			ImageInformation info = new ImageInformation();

			Texture texture = TextureLoader.FromStream(GUIGraphicsContext.DX9Device,
																			 stm,
																			 width, height,
																			 1,
																			 0,
																			 Format.A8R8G8B8,
																			 Pool.Default,
																			 Filter.None,
																			 Filter.None,
																			 (int)0,
																			 ref info);
			stm.Close();

			return texture;
		}

		private void LoadTextureFromAspect(TestPatternApect tpAspect, int testPatternIndex)
		{
			TestPattern tp = _testPatterns.TestPatterns[testPatternIndex];

			int width = 1920;
			int height = 0;

			String tpFile = TestPatternList.GetPath();

			switch (tpAspect)
			{
				case TestPatternApect.f16by9:

					tpFile += tp.Image16by9;
					height = 1080;
					break;

				case TestPatternApect.f21by9:

					tpFile += tp.Image21by9;
					height = 800;
					break;
			}

			_texture[0] = LoadTexture(tpFile, width, height);

			if (tp.IsAlternating)
			{
				tpFile = TestPatternList.GetPath();

				switch (tpAspect)
				{
					case TestPatternApect.f16by9:

						tpFile += tp.Image16by9x;
						height = 1080;
						break;

					case TestPatternApect.f21by9:

						tpFile += tp.Image21by9x;
						height = 800;
						break;
				}

				_texture[1] = LoadTexture(tpFile, width, height);
			}
		}

		public void UpdateAspectRatio(int width, int height, int testPatternIndex)
		{
			bool updateNeeded = false;

			if (_texture[0] == null)
				updateNeeded = true;

			TestPatternApect newAspect = TestPatternApect.f16by9;

			double aspectRatio = (double)width / height;

			if (aspectRatio >= 2)
			{
				if (_tpAspect != TestPatternApect.f21by9)
					updateNeeded = true;

				newAspect = TestPatternApect.f21by9;
			}
			else
			{
				if (_tpAspect != TestPatternApect.f16by9)
					updateNeeded = true;
			}

			if (testPatternIndex != _testPatternIndex)
				updateNeeded = true;

			if (updateNeeded)
			{
				Dispose();

				LoadTextureFromAspect(newAspect, testPatternIndex);

				_tpAspect = newAspect;
				_testPatternIndex = testPatternIndex;
			}
		}

		public void Render(Rectangle sourceRect, Rectangle targetRect, Surface backbuffer, GUIGraphicsContext.eRender3DModeHalf renderModeHalf)
		{
			if (renderModeHalf == GUIGraphicsContext.eRender3DModeHalf.None ||
				  renderModeHalf == GUIGraphicsContext.eRender3DModeHalf.SBSLeft ||
				  renderModeHalf == GUIGraphicsContext.eRender3DModeHalf.TABTop)
			{
				TestPattern tp = _testPatterns.TestPatterns[_testPatternIndex];

				if (tp.IsAlternating && _texture[1] != null)
				{
					_countToggle++;

					if (_countToggle > GUIGraphicsContext.DX9Device.DisplayMode.RefreshRate * tp.AlternatingTime)
					{
						_countToggle = 0;

						_textureIndex++;

						if (_textureIndex > 1)
							_textureIndex = 0;
					}
				}
				else
					_textureIndex = 0;
			}

			Surface surfaceTestPattern = _texture[_textureIndex].GetSurfaceLevel(0);

			if (GUIGraphicsContext.IsPlayingVideo && GUIGraphicsContext.IsFullScreenVideo &&
					(surfaceTestPattern.Description.Width != GUIGraphicsContext.VideoSize.Width ||
					surfaceTestPattern.Description.Height != GUIGraphicsContext.VideoSize.Height))
			{
				double dx = (double)surfaceTestPattern.Description.Width / GUIGraphicsContext.VideoSize.Width;
				double dy = (double)surfaceTestPattern.Description.Height / GUIGraphicsContext.VideoSize.Height;

				sourceRect = new Rectangle((int)(sourceRect.X * dx), (int)(sourceRect.Y * dy), (int)(sourceRect.Width * dx), (int)(sourceRect.Height * dy));
			}

			GUIGraphicsContext.DX9Device.StretchRectangle(surfaceTestPattern,
																											sourceRect,
																											backbuffer,
																											targetRect,
																											TextureFilter.Point);
			surfaceTestPattern.Dispose();

			if (renderModeHalf == GUIGraphicsContext.eRender3DModeHalf.None)
				return;

			GUIFont font = GUIFontManager.GetFont(7);

			if (font != null)
				font.DrawText(GUIGraphicsContext.Width / 2 - GUIGraphicsContext.Width * 0.24f, GUIGraphicsContext.Height / 2 + 4, 0xffffffff, renderModeHalfText[(int)renderModeHalf], GUIControl.Alignment.ALIGN_LEFT, -1);
		}

		public void Dispose()
		{
			if (_texture[0] != null)
			{
				_texture[0].Dispose();
				_texture[0] = null;
			}

			if (_texture[1] != null)
			{
				_texture[1].Dispose();
				_texture[1] = null;
			}
		}
	}
}
