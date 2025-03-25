using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradeVision
{
    public class AnswerSheetAnalyzer
    {
        public Bitmap loadAnswerSheet(Bitmap rawAnswerSheetBitmap)
        {   
            var loadedMat = BitmapExtension.ToMat(rawAnswerSheetBitmap);

            return loadedMat.ToBitmap();
        }
    }
}
 