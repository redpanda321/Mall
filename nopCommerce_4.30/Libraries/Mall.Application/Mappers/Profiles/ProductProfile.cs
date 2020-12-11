using AutoMapper;
using Mall.Entities;
using System.Collections.Generic;

namespace Mall.Application.Mappers.Profiles
{
    public class ProductProfile : Profile
    {
       public ProductProfile()
        {
          

            CreateMap<ProductInfo, DTO.Product.Product>();
            CreateMap<DTO.Product.Product, ProductInfo>();

            CreateMap<ProductAttributeInfo, DTO.ProductAttribute>();
            CreateMap<DTO.ProductAttribute, ProductAttributeInfo>();

            CreateMap<ProductDescriptionInfo, DTO.ProductDescription>();
            CreateMap<DTO.ProductDescription, ProductDescriptionInfo>();

            CreateMap<SKUInfo, DTO.SKU>();
            CreateMap<DTO.SKU, SKUInfo>();

            CreateMap<SpecificationValueInfo, DTO.SpecificationValue>();
            CreateMap<DTO.SpecificationValue, SpecificationValueInfo>();

            CreateMap<Entities.TypeInfo, DTO.ProductType>();
            CreateMap<DTO.ProductType, Entities.TypeInfo>();


            CreateMap<List<BrandInfo>, List<DTO.Brand>>();
            CreateMap<List<DTO.Brand>, List<BrandInfo>>();


            CreateMap<BrandInfo, DTO.Brand>();
            CreateMap<DTO.Brand, BrandInfo>();


          

            CreateMap<ShopBrandApplyInfo, DTO.ShopBrandApply>();
            CreateMap<DTO.ShopBrandApply, ShopBrandApplyInfo>();

            CreateMap<ProductCommentImageInfo, DTO.ProductCommentImage>();
            CreateMap<DTO.ProductCommentImage, ProductCommentImageInfo>();

            
            CreateMap<DTO.ProductComment, ProductCommentInfo>().ForMember(p => p.ProductCommentImageInfo, config => config.MapFrom(p => p.Images));
            CreateMap<ProductCommentInfo, DTO.ProductComment>().ForMember(p => p.Images, config => config.MapFrom(p => p.ProductCommentImageInfo));

            CreateMap<ProductRelationProductInfo, DTO.ProductRelationProduct>();
            CreateMap<DTO.ProductRelationProduct, ProductRelationProductInfo>();

            CreateMap<SellerSpecificationValueInfo, DTO.SellerSpecificationValue>();
            CreateMap<DTO.SellerSpecificationValue, SellerSpecificationValueInfo>();

            CreateMap<ProductDescriptionInfo, DTO.ProductDescription>();
            CreateMap<DTO.ProductDescription, ProductDescriptionInfo>();

            CreateMap<ProductDescriptionTemplateInfo, DTO.ProductDescriptionTemplate>();
            CreateMap<DTO.ProductDescriptionTemplate, ProductDescriptionTemplateInfo>();

            CreateMap<ShopCategoryInfo, DTO.ShopCategory>();
            CreateMap<DTO.ShopCategory, ShopCategoryInfo>();

            CreateMap<ProductShopCategoryInfo, DTO.ProductShopCategory>();
            CreateMap<DTO.ProductShopCategory, ProductShopCategoryInfo>();
        }
    }
}
