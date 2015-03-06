using MediaPortal.GUI.Library;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MediaPortal.Player
{    
    public enum TestPatternApect { f16by9, f21by9 }

    public class TestPattern
    {        
        Texture _texture = null;

        TestPatternApect _tpAspect;
        int _testPatternIndex;

        private String[] renderModeHalfText = { "", "Left Image", "Right Image", "Top Image", "Bottom Image" };

        public TestPattern()
        {           
        }

        private Texture LoadTextureFromAspect(TestPatternApect tpAspect, int testPatternIndex)
        {
            String tpName = "TestPattern_" + (testPatternIndex + 1) + "_";

            int width = 1920;
            int height = 1080;

            switch (tpAspect)
            {
                case TestPatternApect.f16by9:

                    tpName += "16x9";
                    break;

                case TestPatternApect.f21by9:

                    tpName += "21x9";
                    height = 800;
                    break;
            }

            tpName += ".png";

            Stream stm = Assembly.GetExecutingAssembly().GetManifestResourceStream("MediaPortal.Player.TestPattern." + tpName);

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

            return texture;           
        }

        public void UpdateAspectRatio(int width, int height, int testPatternIndex)
        {
            bool updateNeeded = false;

            if (_texture == null)
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
                if (_texture != null)
                    _texture.Dispose();

                _texture = LoadTextureFromAspect(newAspect, testPatternIndex);

                _tpAspect = newAspect;
                _testPatternIndex = testPatternIndex;
            }
        }

        public void Render(Rectangle sourceRect, Rectangle targetRect, Surface backbuffer, GUIGraphicsContext.eRender3DModeHalf renderModeHalf)
        {
            Surface surfaceTestPattern = _texture.GetSurfaceLevel(0);

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
            _texture.Dispose();
        }
    }
}
