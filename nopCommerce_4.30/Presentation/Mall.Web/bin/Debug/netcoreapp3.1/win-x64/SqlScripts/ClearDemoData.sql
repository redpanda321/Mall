set foreign_key_checks=0;
DELETE FROM Mall_accountdetail WHERE ID>=532 AND ID<=546;
DELETE FROM Mall_account WHERE ID>=157 AND ID<=162;
DELETE FROM Mall_activemarketservice WHERE ID>=38 AND ID<=42;

DELETE FROM Mall_articlecategory WHERE ID>=9 AND ID<=17;

DELETE FROM Mall_article WHERE ID>=88 AND ID<=92;

DELETE FROM Mall_attributevalue WHERE ID>=810 AND ID<=852;
DELETE FROM Mall_attribute WHERE ID>=192 AND ID<=204;

DELETE FROM Mall_banner WHERE ID>=73 AND ID<=81 AND  DisplaySequence>0;

DELETE FROM Mall_brand WHERE ID>=319 AND ID<=367 ;
/*
DELETE FROM Mall_category WHERE ID>=1 AND ID<=159 ;
*/
DELETE FROM Mall_categorycashdeposit WHERE CategoryId>=1 AND CategoryId<=159 ;

DELETE FROM Mall_chargedetailshop WHERE  ID in ('17021411310728770','17021411323326291');

/*×éºÏ¹º*/
DELETE c FROM Mall_collocationporuduct a INNER JOIN Mall_collocation b on a.colloid=b.id
INNER JOIN Mall_collocationsku c on c.colloproductid=a.id
WHERE  b.Id>=21 AND b.Id<=25
;
DELETE a FROM Mall_collocationporuduct a INNER JOIN Mall_collocation b on a.colloid=b.id
WHERE  b.Id>=21 AND b.Id<=25
;
DELETE FROM Mall_collocation WHERE ID>=21 AND ID<=25
;
DELETE b FROM Mall_Coupon a INNER JOIN Mall_couponsetting b on a.Id=b.couponid WHERE  a.Id>=59 AND a.Id<=64;
DELETE FROM Mall_Coupon WHERE Id>=59 AND Id<=64;

DELETE FROM Mall_couponrecord WHERE ID IN (884,885);


DELETE FROM Mall_fightgroupactiveitem WHERE ActiveId>=26 AND ActiveId<=30 ;
DELETE FROM Mall_fightgroupactive WHERE  Id>=26 AND Id<=30;

DELETE FROM Mall_flashsaledetail WHERE  FlashSaleId>=26 AND FlashSaleId<=30 ;
DELETE FROM Mall_flashsale WHERE Id>=34 AND Id<=50 ;
DELETE FROM Mall_flashsaleconfig  ;

/*
DELETE b FROM Mall_freighttemplate a INNER JOIN Mall_freightareadetail b on a.Id=b.FreightTemplateId
WHERE  a.Id>=166 AND a.Id<=169
;
DELETE b FROM Mall_freighttemplate a INNER JOIN Mall_freightareacontent b on a.Id=b.FreightTemplateId
WHERE  a.Id>=166 AND a.Id<=169
;
DELETE FROM Mall_freighttemplate WHERE Id>=166 AND Id<=169 ;
*/
DELETE FROM Mall_gift WHERE Id>=68 AND Id<=77 ;

DELETE FROM Mall_integralmallad WHERE Id>=3 AND Id<=4 ;

DELETE FROM Mall_label WHERE Id>=33 AND Id<=35 ;

DELETE FROM Mall_log WHERE Id>=4608 AND Id<=5024 ;

DELETE FROM Mall_marketservicerecord WHERE Id>=101 AND Id<=107 ;
DELETE FROM Mall_marketsettingmeta WHERE Id>=5 AND Id<=5 ;

DELETE FROM Mall_memberintegralexchangerule WHERE Id>=2 AND Id<=2 ;

DELETE FROM Mall_menu WHERE Id>=65 AND Id<=72 ;

DELETE FROM Mall_messagelog WHERE Id>=875 AND Id<=878 ;

DELETE FROM Mall_mobilehomeproduct WHERE Id>=104 AND Id<=134 ;

DELETE FROM Mall_moduleproduct WHERE Id>=723 AND Id<=963 ;

DELETE FROM Mall_photospace WHERE Id>=102 AND Id<=538 ;
DELETE FROM Mall_photospacecategory WHERE Id>=3 AND Id<=3 ;

DELETE FROM Mall_plataccountitem WHERE AccoutId>=1 AND AccoutId<=1 ;

DELETE FROM Mall_plataccount WHERE Id>=1 AND Id<=1 ;

DELETE FROM Mall_productattribute WHERE  ProductId>=699 AND ProductId<=749 ;
DELETE FROM Mall_productdescription WHERE ProductId>=699 AND ProductId<=749 ;
DELETE FROM Mall_sku  WHERE ProductId>=699 AND ProductId<=749 ;
DELETE FROM Mall_product WHERE Id>=699 AND Id<=749 ;
DELETE FROM Mall_browsinghistory WHERE ProductId>=699 AND ProductId<=749 ;
DELETE FROM Mall_favorite WHERE ProductId>=699 AND ProductId<=749 ;


DELETE FROM Mall_productshopcategory WHERE shopcategoryid>=350 AND shopcategoryid<=360 ;
/*DELETE FROM Mall_shopcategory WHERE Id>=350 AND Id<=360 ;*/

DELETE FROM Mall_receivingaddressconfig WHERE Id>=1 AND Id<=1 ;
DELETE FROM Mall_refundreason WHERE Id>=28 AND Id<=30 ;

DELETE FROM Mall_roleprivilege WHERE roleid>=46 AND roleid<=49 ;
DELETE FROM Mall_role WHERE Id>=46 AND Id<=49 ;

DELETE FROM Mall_settled WHERE Id>=2 AND Id<=2 ;


DELETE FROM Mall_shopbonus WHERE Id>=8 AND Id<=8 ;

DELETE FROM Mall_shopaccountitem WHERE shopid>=1 AND shopid<=1 ;
DELETE FROM Mall_shopaccount WHERE Id>=1 AND Id<=1 ;

DELETE FROM Mall_shopbranchmanager WHERE  shopbranchId>=26 AND shopbranchId<=26 ;
DELETE FROM Mall_shopbranchsku WHERE ShopBranchId>=26 AND ShopBranchId<=26 ;
DELETE FROM Mall_ShopBranch WHERE  ShopID>=1 AND ShopID<=1 ;

DELETE FROM Mall_shopfooter WHERE Id>=19 AND Id<=19 ;

DELETE FROM Mall_shophomemoduleproduct WHERE homemoduleid>=29 AND homemoduleid<=31 ;
DELETE FROM Mall_shophomemodule WHERE Id>=29 AND Id<=31 ;

DELETE FROM Mall_sitesigninconfig WHERE Id>=2 AND Id<=2 ;

DELETE FROM Mall_slidead  WHERE Id>=105 AND Id<=132 ;

/*DELETE FROM Mall_specificationvalue  WHERE Id>=646 AND Id<=681 ;*/

DELETE FROM Mall_topicmodule WHERE topicid>=54 AND topicid<=63 ;
DELETE FROM Mall_topic WHERE id>=54 AND id<=63;

/*DELETE FROM Mall_typebrand WHERE typeid>=82 AND typeid<=96;*/

/*DELETE FROM Mall_Type WHERE Id>=82 AND Id<=96 ;*/

DELETE FROM Mall_weiactivityaward WHERE activityid>=154 AND activityid<=155;

DELETE FROM Mall_weiactivityinfo WHERE Id>=154 AND Id<=155 ;

DELETE FROM Mall_weixinbasic WHERE Id>=3 AND Id<=3 ;


DELETE FROM Mall_weixinmsgtemplate WHERE Id>=41 AND Id<=58 ;

DELETE FROM Mall_homecategory  WHERE Id>=2450 AND Id<=2518 ;

DELETE FROM Mall_homefloor WHERE Id>=151 AND Id<=167 ;

DELETE FROM Mall_floorbrand WHERE FloorId>=151 AND FloorId<=167 ;

DELETE FROM Mall_floortopic WHERE FloorId>=151 AND FloorId<=167 ;
DELETE FROM Mall_floorproduct WHERE FloorId>=151 AND FloorId<=167 ;


DELETE FROM Mall_searchproduct WHERE ProductId>=699 AND ProductId<=749 ;

UPDATE `Mall_SiteSetting` SET `Value`='false' WHERE `Key`='IsCanClearDemoData';

set foreign_key_checks=1;