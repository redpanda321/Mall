using NPoco;
using System.Configuration;

namespace Mall.Entities
{
    public partial class TopicInfo
    {
        protected string ImageServerUrl = "";
        [ResultColumn]
        public string TopImageUrl
        {
            get { return Core.MallIO.GetImagePath(TopImage); }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    TopImage = value.Replace(ImageServerUrl, "");
                else
                    TopImage = value;
            }
        }

        [ResultColumn]
        public string BackgroundImageUrl
        {
            get { return  Core.MallIO.GetImagePath(BackgroundImage); }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    BackgroundImage = value.Replace(ImageServerUrl, "");
                else
                    BackgroundImage = value;
            }
        }

        [ResultColumn]
        public string FrontCoverImageUrl
        {
            get { return  Core.MallIO.GetImagePath(FrontCoverImage); }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    FrontCoverImage = value.Replace(ImageServerUrl, "");
                else
                    FrontCoverImage = value;
            }
        }


    }
}
