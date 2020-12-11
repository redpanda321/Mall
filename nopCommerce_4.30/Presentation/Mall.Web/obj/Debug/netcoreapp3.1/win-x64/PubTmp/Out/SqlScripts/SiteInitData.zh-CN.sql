-- ----------------------------
-- Records of Mall_ArticleCategories
-- ----------------------------
set foreign_key_checks=0;
INSERT INTO `Mall_ArticleCategory` VALUES ('1', '0', '底部帮助', '1', '1');
INSERT INTO `Mall_ArticleCategory` VALUES ('2', '0', '系统快报', '2', '1');
INSERT INTO `Mall_ArticleCategory` VALUES ('3', '0', '商城公告', '3', '1');
INSERT INTO `Mall_ArticleCategory` VALUES ('4', '0', '商家后台公告', '4', '1');
INSERT INTO `Mall_ArticleCategory` VALUES ('9', '1', '购物指南', '1', '0');
INSERT INTO `Mall_ArticleCategory` VALUES ('10', '1', '店主之家', '1', '0');
INSERT INTO `Mall_ArticleCategory` VALUES ('11', '1', '支付方式', '1', '0');
INSERT INTO `Mall_ArticleCategory` VALUES ('12', '1', '售后服务', '1', '0');
INSERT INTO `Mall_ArticleCategory` VALUES ('13', '1', '关于我们', '1', '0');
INSERT INTO `Mall_ArticleCategory` VALUES ('14', '0', '保障服务', '1', '1');
INSERT INTO `Mall_ArticleCategory` VALUES ('15', '14', '七天无理由', '1', '1');
INSERT INTO `Mall_ArticleCategory` VALUES ('16', '14', '消费者保障', '1', '1');
INSERT INTO `Mall_ArticleCategory` VALUES ('17', '14', '及时发货服', '1', '1');






-- ----------------------------
-- Records of Mall_ImageAd
-- ----------------------------
INSERT INTO `Mall_ImageAd` VALUES (1, 0, '/Storage/Plat/ImageAd/201507141112539072470.jpg', '/',0,4);
INSERT INTO `Mall_ImageAd` VALUES (2, 0, '/Storage/Plat/ImageAd/201507141113517447420.jpg', '/',0,4);
INSERT INTO `Mall_ImageAd` VALUES (3, 0, '/Storage/Plat/ImageAd/201507141113577374740.jpg', '/',0,4);
INSERT INTO `Mall_ImageAd` VALUES (4, 0, '/Storage/Plat/ImageAd/201507141114038361740.jpg', '/',0,4);
INSERT INTO `Mall_ImageAd` VALUES (5, 0, '/Storage/Plat/ImageAd/201507141114107154480.jpg', '/',0,4);
INSERT INTO `Mall_ImageAd` VALUES (6, 0, '/Storage/Plat/ImageAd/201507141114450875180.jpg', '/',0,4);
INSERT INTO `Mall_ImageAd` VALUES (7, 0, '/Storage/Plat/ImageAd/201507141114557255630.jpg', '/',0,4);
INSERT INTO `Mall_ImageAd` VALUES (8, 0, '/Storage/Plat/ImageAd/201507141115026562490.jpg', '/',0,4);
INSERT INTO `Mall_ImageAd` VALUES (9, 0, '/Storage/Plat/ImageAd/201411271925179346361.png', '/',0,0);
INSERT INTO `Mall_ImageAd` VALUES (10, 0, '/Storage/Plat/ImageAd/201411051538294152167.jpg', '/',0,2);
INSERT INTO `Mall_ImageAd` VALUES (11, 0, '/Storage/Plat/ImageAd/201411051538347344891.jpg', '/',0,3);
INSERT INTO `Mall_ImageAd` VALUES (12, 0, '/Storage/Plat/ImageAd/201411051538401142128.jpg', '/',0,3);
INSERT INTO `Mall_ImageAd` VALUES (13, 0, '/Storage/Plat/ImageAd/201411271923150932253.jpg', '/',0,3);
INSERT INTO `Mall_ImageAd` VALUES (14, 0, '/Storage/Plat/ImageAd/201507141154486858880.jpg', '/',0,1);
INSERT INTO `Mall_ImageAd` VALUES (15, 0, '/Storage/Plat/ImageAd/201507141125025515100.jpg', '/',0,1);
INSERT INTO `Mall_ImageAd` VALUES (16, 0, '/Storage/Plat/ImageAd/201509171117596463730.jpg', '',0,6);
INSERT INTO `Mall_ImageAd` VALUES (17, 0, '/Storage/Plat/ImageAd/201509171118078648430.jpg', '',0,6);
INSERT INTO `Mall_ImageAd` VALUES (18, 0, '/Storage/Plat/ImageAd/201509171118158693010.jpg', '',0,6);
INSERT INTO `Mall_ImageAd` VALUES (19, 0, '/Storage/Plat/ImageAd/201509171118225416830.jpg', '',0,6);
INSERT INTO `Mall_ImageAd` VALUES (20, 0, '/Storage/Plat/ImageAd/201509171118295840850.jpg', '',0,6);
INSERT INTO `Mall_ImageAd` VALUES (21, 0, '/Storage/Plat/ImageAd/201509171118295840850.jpg', '',0,5);
INSERT INTO `Mall_ImageAd` VALUES (22, 0, '/Storage/Plat/ImageAd/201509171118295840850.jpg', '',0,5);
-- ----------------------------
-- Records of Mall_SiteSetting
-- ----------------------------
INSERT INTO `Mall_SiteSetting` VALUES ('1', 'Logo', '/Storage/Plat/ImageAd/logo.jpg');
INSERT INTO `Mall_SiteSetting` VALUES ('2', 'SiteName', '站点1号');
INSERT INTO `Mall_SiteSetting` VALUES ('3', 'ICPNubmer', ' ');
INSERT INTO `Mall_SiteSetting` VALUES ('4', 'CustomerTel', ' ');
INSERT INTO `Mall_SiteSetting` VALUES ('5', 'SiteIsOpen', 'False');
INSERT INTO `Mall_SiteSetting` VALUES ('6', 'Keyword', '手机');
INSERT INTO `Mall_SiteSetting` VALUES ('7', 'Hotkeywords', '男装,海飞丝,女装,ABC,手机,Nikon,包包,鞋子');
INSERT INTO `Mall_SiteSetting` VALUES ('8', 'PageFoot', ' ');
INSERT INTO `Mall_SiteSetting` VALUES ('10', 'MemberLogo', '');
INSERT INTO `Mall_SiteSetting` VALUES ('11', 'QRCode', '');
INSERT INTO `Mall_SiteSetting` VALUES ('12', 'FlowScript', 'FlowScript1');
INSERT INTO `Mall_SiteSetting` VALUES ('13', 'Site_SEOTitle', 'Site_SEOTitle1');
INSERT INTO `Mall_SiteSetting` VALUES ('14', 'Site_SEOKeywords', 'Site_SEOKeywords1');
INSERT INTO `Mall_SiteSetting` VALUES ('15', 'Site_SEODescription', 'Site_SEODescription1');
INSERT INTO `Mall_SiteSetting` VALUES ('16', 'ProdutAuditOnOff', '1');
INSERT INTO `Mall_SiteSetting` VALUES ('17', 'WithDrawMinimum', '1');
INSERT INTO `Mall_SiteSetting` VALUES ('18', 'WithDrawMaximum', '2000');
INSERT INTO `Mall_SiteSetting` VALUES ('19', 'WeekSettlement', '1');
INSERT INTO `Mall_SiteSetting` (`Key`, `Value`) VALUES ('IsOpenStore', 'true');
INSERT INTO `Mall_SiteSetting` (`Key`, `Value`) VALUES ('IsOpenShopApp', 'True');
INSERT INTO `Mall_SiteSetting` (`Key`, `Value`) VALUES ('WeixinAppletId', '');
INSERT INTO `Mall_SiteSetting` (`Key`, `Value`) VALUES ('WeixinAppletSecret', '');
INSERT INTO `Mall_SiteSetting` (`Key`, `Value`) VALUES ('IsOpenPC', 'True');
INSERT INTO `Mall_SiteSetting` (`Key`, `Value`) VALUES ('IsOpenH5', 'false');
INSERT INTO `Mall_SiteSetting` (`Key`, `Value`) VALUES ('IsOpenApp', 'false');
INSERT INTO `Mall_SiteSetting` (`Key`, `Value`) VALUES ('IsOpenMallSmallProg', 'false');
INSERT INTO `Mall_SiteSetting` (`Key`, `Value`) VALUES ('MallJDVersion', '0.0.0');
INSERT INTO `Mall_SiteSetting` (`Key`, `Value`) VALUES ('ShopWithDrawMinimum', '1');
INSERT INTO `Mall_SiteSetting` (`Key`, `Value`) VALUES ('ShopWithDrawMaximum', '5000');
INSERT INTO `Mall_SiteSetting` (`Key`, `Value`) VALUES ('ProductSaleCountOnOff', '1');

-- ----------------------------
-- Records of Mall_ShopGrade
-- ----------------------------
INSERT INTO `Mall_ShopGrade` VALUES ('1', '白金店铺', '500', '500', '500', '500.00', null);
INSERT INTO `Mall_ShopGrade` VALUES ('2', '钻石店铺', '1000', '1000', '1000', '1000.00', null);


-- ----------------------------
-- Records of Mall_Shop
-- ----------------------------
INSERT INTO `Mall_Shop`
(Id,
GradeId,
ShopName,
Logo,
SubDomains,
Theme,
IsSelf,
ShopStatus,
RefuseReason,
CreateDate,
EndDate,
CompanyName,
CompanyRegionId,
CompanyAddress,
CompanyPhone,
CompanyEmployeeCount,
CompanyRegisteredCapital,
ContactsName,
ContactsPhone,
ContactsEmail,
BusinessLicenceNumber,
BusinessLicenceNumberPhoto,
BusinessLicenceRegionId,
BusinessLicenceStart,
BusinessLicenceEnd,
BusinessSphere,
OrganizationCode,
OrganizationCodePhoto,
GeneralTaxpayerPhot,
BankAccountName,
BankAccountNumber,
BankName,
BankCode,
BankRegionId,
BankPhoto,
TaxRegistrationCertificate,
TaxpayerId,
TaxRegistrationCertificatePhoto,
PayPhoto,
PayRemark,
SenderName,
SenderAddress,
SenderPhone,
Freight,
FreeFreight,
Stage,
SenderRegionId,
BusinessLicenseCert,
ProductCert,
OtherCert,
legalPerson,
CompanyFoundingDate,AutoAllotOrder)
 VALUES ('1', '1', '官方自营店', null, null, null, '1', '7', null, '2014-10-30 00:00:00', ADDDATE(NOW(),INTERVAL 100 YEAR), '海商网络科技', '102', '文化大厦', '876588888', '1000', '1.00', '杨先生', '13988887748', '', '966587458', '1', '102', '2014-05-05 00:00:00', '2014-12-12 00:00:00', '1', '66548726', '1', '1', '杨先生', '6228445888796651200', '中国银行', '44698', '101', '1', '1', '33695', '1', '1', '1', '1', '1', '1', '11.00', '11.00', '5', '102', null, null, null, null, null,0);





-- ----------------------------
-- Records of Mall_Agreement
-- ----------------------------
INSERT INTO `Mall_Agreement` VALUES ('1', 0 , '<div><p><strong>【审慎阅读】</strong>您在申请注册流程中点击同意前，应当认真阅读以下协议。<strongstyle="text-decoration:underline">请您务必审慎阅读、充分理解协议中相关条款内容，其中包括：</strong></p><p>1、<strongstyle="text-decoration:underline">与您约定免除或限制责任的条款；</strong></p><p>2、<strongstyle="text-decoration:underline">与您约定法律适用和管辖的条款；</strong></p><p>3、<strongstyle="text-decoration:underline">其他以粗体下划线标识的重要条款。</strong></p><p>如您对协议有任何疑问，可向平台客服咨询。</p><p><strong>【特别提示】</strong>当您按照注册页面提示填写信息、阅读并同意协议且完成全部注册程序后，即表示您已充分阅读、理解并接受协议的全部内容。如您因平台服务与Mall发生争议的，适用《Mall平台服务协议》处理。如您在使用平台服务过程中与其他用户发生争议的，依您与其他用户达成的协议处理。</p><p><strongstyle="text-decoration:underline">阅读协议的过程中，如果您不同意相关协议或其中任何条款约定，您应立即停止注册程序。</strong></p><p><a target="_blank">《Mall服务协议》</a></p><p><a target="_blank">《法律声明及隐私权政策》</a></p><p><a target="_blank"></a></p><pstyle="text-align:center"></p></div>' , '2015-07-15 16:54:35');
INSERT INTO `Mall_Agreement` VALUES ('2', 1 , '<div class="board_content"><p>	为了更好地开拓市场，更好地为本地客户服务，甲乙双方本着互惠互利的双赢策略，根据乙方崇左商城入驻条件，甲方申请入驻乙方商城，并完全接受乙方的规范，经友好协商，甲乙双方签署以下入驻协议。</p><p>	<br>一、&nbsp;入驻商家基本条件<br>1、&nbsp;有良好的合作意愿，能提供优质的商品，保证合作的顺利进行，并保证提供良好的售后服务；<br>2、&nbsp;甲方必须满足以下条件之一（符合其中一项即可）：<br>⑴授权商，获得国际或者国内知名品牌厂商的授权；<br>⑵拥有自己注册商标的生产型厂商；<br>⑶专业品牌经销商，代理商，B2C网站；<br>⑷专业品牌专卖店。<br>3、&nbsp;甲方应在签订本协议时向乙方提供（经乙方认可的）包括但不限于以下证明材料复印件：<br>⑴营业执照 (包括个体户营业执照)、税务登记证；<br>⑵拥有注册商标或者品牌，或者拥有正规的品牌授权书。<br>4、&nbsp;甲方承诺：<br>⑴所有商品一口价销售（参与打折、促销、秒杀活动除外）；<br>⑵七天无损坏不影响二次销售情况下换货(如商品无质量问题，且包装未破坏不影响二次销售，换货费用由买家承担，每张订单只提供一次换货服务；如属商品质量问题，换货费用由商家承担)；<br>⑶实体店地址变更应及时告知乙方；<br>⑷参加乙方全网积分购物活动；<br>⑸所有商品保证原厂正品，保障商品质量、承诺售后服务，必要时能提供销售发票。</p><p>	<br>二、&nbsp;入驻商铺类型<br>1、&nbsp;展示型商铺；<br>2、&nbsp;销售型商铺。</p><p>	&nbsp;</p><p>	三、入驻商铺等级</p><p>	1、普通商铺</p><p>	2、扶持商铺；</p><p>	3、拓展商铺；</p><p>	4、旗舰商铺。</p><p>	<br>三、&nbsp;商铺入驻开通流程<br>1、&nbsp;甲乙双方签署本协议；<br>2、&nbsp;甲方根据所选择商铺类型，提交相应的证明材料复印件；<br>3、&nbsp;在乙方非常商城注册，获取管理员账号；<br>4、&nbsp;乙方审核通过后，甲方即可发布商品并展示经营活动。</p><p>	<br>四、&nbsp;双方权利义务<br>甲方权利义务：<br>1、&nbsp;在本协议第一条规定范围内，甲方自行开拓市场与发展业务，不得以欺诈、胁迫等不正当手段损害客户或甲方的利益与声誉；<br>2、&nbsp;甲方妥善保管商铺管理员账号，如因甲方保管、设置或使用不当造成的经济损失，由甲方自行承担责任；<br>3、&nbsp;甲方在使用过程中如发现任何非法盗用乙方账号出现安全漏洞等情况，应立即通知乙方<br>4、&nbsp;甲方保证其所有经营活动完全符合中国有关法律、法规、行政规章等的规定。如因甲方违反上述规定给乙方带来任何损害，甲方应承担所有法律责任并赔偿由此给甲方造成的损失；<br>5、&nbsp;在协议有效期内，甲方不得向与乙方构成商业竞争关系的企业、商业机构或者组织提供有关乙方相关信息或者资料，否则对乙方造成损失的，由甲方负责赔偿。<br>乙方权利义务：<br>1、&nbsp;乙方对甲方提供必要的开店技术咨询，并保证商城系统平台能正常稳定运行；<br>2、&nbsp;如甲方是托管商铺，乙方应及时为甲方进行店铺更新并实时进行日常管理；<br>3、&nbsp;在本协议有效期限内，乙方有权根据市场情况对各种商铺入驻条件进行调整。</p><p>	<br>五、&nbsp;协议期限<br>本协议有效期为一年，自签订之日起生效。有效期满后，双方可视合作情况续约。</p><p>	六、&nbsp;协议变更、终止及违约责任<br>1、&nbsp;甲乙双方应本着诚实信用的原则履行本协议。任何一方在履约过程中采用欺诈、胁迫或者暴力的手段，另一方均可以解除本协议并要求对方赔偿由此造成的损失；<br>2、&nbsp;在协议执行期间，如果双方或一方认为需要终止，应提前一个月通知对方，双方在财务结算完毕、各自责任明确履行之后，方可终止协议。某方擅自终止本协议，给对方造成损失的，应赔偿对方损失；<br>3、&nbsp;经双方协商达成一致，可以对本协议有关条款进行变更，但应当以书面形式确认；<br>4、&nbsp;一方变更通讯地址或其它联系方式，应自变更之日起十日内，将变更后的地址、联系方式通知另一方，否则变更方应对此造成的一切后果承担责任；<br>5、&nbsp;如因客观情况发生重大变化，致使本协议无继续法履行的，经甲乙双方协商同意，可以变更或者终止协议的履行。</p><p><br>七、&nbsp;保密条款<br>1、甲、乙双方所提供给对方的一切资料、技术和对项目的策划设计要严保密，并只能在合作双方公司的业务范围内使用；<br>2、甲、乙双方均应对在合作过程中所知悉的对的商业和技术秘密承担保密义务。保密条款不受本协议期限的限制。</p><p>	<br>八、&nbsp;争解决<br>在本协议执行期间如果双方发生争议，双方应友好协商解决。如果协商不成，双方同意提交崇左市仲裁委员会进行仲裁，接受其仲裁规则，并服从该仲裁裁决，仲裁费由败诉方承担。争议的解适用中华人民共和国法律、法规、条例和计算机行业惯例。</p></div>' , '2015-07-15 16:54:35');


-- ----------------------------
-- Records of Mall_InvoiceContext
-- ----------------------------
insert into Mall_InvoiceContext(name) VALUES('个人');

-- ----------------------------
-- Records of Mall_PaymentConfig
-- ----------------------------
INSERT INTO `Mall_PaymentConfig` VALUES ('1', '0');

INSERT INTO `Mall_expressinfo` (`Id`, `Name`, `TaobaoCode`, `Kuaidi100Code`, `KuaidiNiaoCode`, `Width`, `Height`, `Logo`, `BackGroundImage`, `Status`, `CreateDate`) VALUES ('19', '顺丰速递', NULL, 'shunfeng', 'SF', '230', '480', '/Storage/Plat/Express/logo/shunfeng.png', '/Storage/Plat/Express/顺丰速递.png', '1', '0001-01-01 00:00:00');
INSERT INTO `Mall_expressinfo` (`Id`, `Name`, `TaobaoCode`, `Kuaidi100Code`, `KuaidiNiaoCode`, `Width`, `Height`, `Logo`, `BackGroundImage`, `Status`, `CreateDate`) VALUES ('20', '圆通速递', NULL, 'yuantong', 'YTO', '230', '480', '/Storage/Plat/Express/logo/yuantong.png', '/Storage/Plat/Express/圆通速递.png', '1', '0001-01-01 00:00:00');
INSERT INTO `Mall_expressinfo` (`Id`, `Name`, `TaobaoCode`, `Kuaidi100Code`, `KuaidiNiaoCode`, `Width`, `Height`, `Logo`, `BackGroundImage`, `Status`, `CreateDate`) VALUES ('21', '申通快递', NULL, 'shentong', 'STO', '230', '480', '/Storage/Plat/Express/logo/shentong.png', '/Storage/Plat/Express/申通快递.png', '1', '0001-01-01 00:00:00');
INSERT INTO `Mall_expressinfo` (`Id`, `Name`, `TaobaoCode`, `Kuaidi100Code`, `KuaidiNiaoCode`, `Width`, `Height`, `Logo`, `BackGroundImage`, `Status`, `CreateDate`) VALUES ('22', '中通快递', NULL, 'zhongtong', 'ZTO', '230', '480', '/Storage/Plat/Express/logo/zhongtong.png', '/Storage/Plat/Express/中通快递.png', '1', '0001-01-01 00:00:00');
INSERT INTO `Mall_expressinfo` (`Id`, `Name`, `TaobaoCode`, `Kuaidi100Code`, `KuaidiNiaoCode`, `Width`, `Height`, `Logo`, `BackGroundImage`, `Status`, `CreateDate`) VALUES ('23', '韵达速递', NULL, 'yunda', 'YD', '230', '480', '/Storage/Plat/Express/logo/yunda.png', '/Storage/Plat/Express/韵达速递.png', '1', '0001-01-01 00:00:00');
INSERT INTO `Mall_expressinfo` (`Id`, `Name`, `TaobaoCode`, `Kuaidi100Code`, `KuaidiNiaoCode`, `Width`, `Height`, `Logo`, `BackGroundImage`, `Status`, `CreateDate`) VALUES ('24', 'EMS', NULL, 'ems', 'EMS', '230', '480', '/Storage/Plat/Express/logo/ems.png', '/Storage/Plat/Express/EMS.png', '1', '0001-01-01 00:00:00');
INSERT INTO `Mall_expressinfo` (`Id`, `Name`, `TaobaoCode`, `Kuaidi100Code`, `KuaidiNiaoCode`, `Width`, `Height`, `Logo`, `BackGroundImage`, `Status`, `CreateDate`) VALUES ('25', '百世汇通', NULL, 'huitongkuaidi', 'HTKY', '230', '480', '/Storage/Plat/Express/logo/BaiShiHuiTong.png', '/Storage/Plat/Express/百世汇通.png', '1', '0001-01-01 00:00:00');
INSERT INTO `Mall_expressinfo` (`Id`, `Name`, `TaobaoCode`, `Kuaidi100Code`, `KuaidiNiaoCode`, `Width`, `Height`, `Logo`, `BackGroundImage`, `Status`, `CreateDate`) VALUES ('26', '天天快递', NULL, 'tiantian', 'HHTT', '230', '480', '/Storage/Plat/Express/logo/tiantian.png', '/Storage/Plat/Express/天天快递.png', '1', '0001-01-01 00:00:00');
INSERT INTO `Mall_expressinfo` (`Id`, `Name`, `TaobaoCode`, `Kuaidi100Code`, `KuaidiNiaoCode`, `Width`, `Height`, `Logo`, `BackGroundImage`, `Status`, `CreateDate`) VALUES ('27', '邮政平邮', NULL, 'youzhengguonei', 'YZPY', '230', '480', '', '/Storage/Plat/Express/邮政平邮.png', '1', '0001-01-01 00:00:00');
INSERT INTO `Mall_expressinfo` (`Id`, `Name`, `TaobaoCode`, `Kuaidi100Code`, `KuaidiNiaoCode`, `Width`, `Height`, `Logo`, `BackGroundImage`, `Status`, `CreateDate`) VALUES ('28', '宅急送', NULL, 'zhaijisong', 'ZJS', '230', '480', '/Storage/Plat/Express/logo/zhaijisong.png', '/Storage/Plat/Express/宅急送.png', '1', '0001-01-01 00:00:00');


INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('94', '20', '1', '4423', '5312', '5645', '6104');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('95', '20', '2', '4274', '3583', '7534', '4729');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('96', '20', '3', '6497', '5270', '7684', '6208');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('97', '20', '5', '4562', '2937', '5714', '3562');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('98', '20', '6', '5599', '2958', '6751', '3583');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('99', '20', '7', '6451', '2958', '7603', '3583');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('100', '20', '8', '1025', '5333', '2269', '6229');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('101', '20', '9', '1036', '2937', '2188', '3562');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('102', '20', '10', '2039', '2937', '3191', '3562');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('103', '20', '11', '2972', '2937', '4124', '3562');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('104', '20', '12', '817', '3645', '4055', '4708');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('105', '20', '13', '2891', '5416', '4170', '6270');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('106', '20', '15', '1336', '6854', '4112', '7770');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('107', '20', '19', '4631', '6145', '7615', '7520');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('108', '20', '21', '1520', '4666', '4158', '5770');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('109', '19', '21', '1036', '2645', '2684', '3395');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('110', '19', '8', '3006', '2666', '4274', '3416');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('111', '19', '12', '967', '3250', '3894', '4270');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('112', '19', '13', '1716', '4145', '3410', '5020');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('113', '19', '1', '3029', '4916', '4182', '5666');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('114', '19', '5', '898', '5562', '2050', '6187');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('115', '19', '6', '1889', '5562', '3041', '6187');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('116', '19', '7', '2891', '5583', '4043', '6208');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('117', '19', '2', '610', '6125', '3894', '7270');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('118', '19', '3', '1947', '7020', '3721', '7770');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('119', '19', '19', '564', '8020', '3801', '8916');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('120', '19', '18', '5979', '9104', '8052', '10062');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('121', '19', '15', '541', '8541', '3847', '9208');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('122', '21', '8', '1347', '1770', '2615', '2645');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('123', '21', '21', '1244', '2562', '4481', '3479');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('124', '21', '9', '1221', '3312', '2373', '3937');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('125', '21', '10', '2235', '3312', '3387', '3937');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('126', '21', '11', '3237', '3312', '4389', '3937');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('127', '21', '12', '691', '3854', '4527', '4854');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('128', '21', '13', '1566', '4604', '4078', '5562');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('129', '21', '19', '656', '6458', '3099', '7583');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('130', '21', '15', '3087', '6687', '5230', '7520');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('131', '21', '1', '5345', '1770', '6774', '2645');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('132', '21', '5', '5161', '3312', '6313', '3937');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('133', '21', '6', '6232', '3312', '7384', '3937');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('134', '21', '7', '7258', '3312', '8410', '3937');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('135', '21', '2', '4735', '3895', '8605', '4833');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('136', '21', '3', '5622', '4583', '8271', '5458');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('137', '22', '8', '1566', '2125', '2811', '2937');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('138', '22', '9', '1532', '2833', '2684', '3458');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('139', '22', '10', '2523', '2833', '3675', '3458');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('140', '22', '11', '3410', '2812', '4562', '3437');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('141', '22', '12', '1497', '3333', '4550', '4375');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('142', '22', '21', '1474', '4104', '4516', '5104');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('143', '22', '13', '1520', '4937', '2949', '5812');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('144', '22', '1', '5506', '2125', '6774', '2895');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('145', '22', '5', '5437', '2875', '6589', '3500');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('146', '22', '6', '6244', '2895', '7396', '3520');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('147', '22', '7', '7246', '2895', '8398', '3520');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('148', '22', '2', '5437', '3395', '8444', '4708');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('149', '22', '3', '5483', '4812', '7062', '5791');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('150', '22', '19', '2718', '6208', '4758', '7312');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('151', '22', '15', '3421', '5604', '5794', '6437');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('152', '22', '18', '4274', '7375', '5944', '8395');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('153', '23', '8', '1255', '1625', '2442', '2395');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('154', '23', '21', '1405', '2270', '4539', '2979');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('155', '23', '9', '1059', '3000', '2211', '3625');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('156', '23', '10', '2073', '3000', '3225', '3625');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('157', '23', '11', '3122', '3000', '4274', '3625');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('158', '23', '12', '898', '3625', '4585', '4583');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('159', '23', '13', '1670', '4500', '3029', '5312');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('160', '23', '3', '7534', '1666', '8824', '2416');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('161', '23', '5', '5126', '2833', '6278', '3458');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('162', '23', '6', '6186', '2833', '7338', '3458');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('163', '23', '7', '7258', '2812', '8410', '3437');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('164', '23', '2', '5080', '3437', '8640', '4541');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('165', '23', '1', '5253', '4500', '6981', '5375');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('166', '23', '19', '771', '6125', '4527', '6937');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('167', '23', '18', '5311', '8229', '7246', '8958');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('168', '23', '15', '2845', '6541', '5817', '7645');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('169', '24', '8', '1463', '2333', '2661', '3125');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('170', '24', '13', '3387', '2333', '4769', '3166');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('171', '24', '1', '5564', '2333', '6751', '3166');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('172', '24', '3', '7776', '2312', '9147', '3187');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('173', '24', '21', '2039', '3000', '4700', '3854');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('174', '24', '9', '1543', '3625', '2695', '4250');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('175', '24', '10', '2546', '3645', '3698', '4270');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('176', '24', '11', '3571', '3625', '4723', '4250');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('177', '24', '12', '852', '4145', '4723', '5041');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('178', '24', '5', '5691', '3604', '6843', '4229');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('179', '24', '6', '6797', '3604', '7949', '4229');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('180', '24', '7', '7891', '3604', '9043', '4229');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('181', '24', '2', '4919', '4145', '9009', '5145');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('182', '24', '19', '829', '6333', '4158', '7395');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('183', '24', '15', '898', '7125', '3410', '8270');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('184', '24', '18', '5426', '7875', '8986', '8916');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('185', '25', '8', '1255', '1895', '2615', '2645');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('186', '25', '21', '1186', '2479', '3986', '3270');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('187', '25', '9', '1336', '3062', '2488', '3687');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('188', '25', '10', '2281', '3083', '3433', '3708');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('189', '25', '11', '3248', '3083', '4400', '3708');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('190', '25', '12', '679', '3645', '4377', '4895');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('191', '25', '13', '1693', '4708', '3421', '5583');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('192', '25', '19', '610', '6104', '3041', '7312');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('193', '25', '15', '610', '6937', '3548', '7937');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('194', '25', '18', '6716', '7479', '8248', '8750');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('195', '25', '1', '5057', '1937', '6520', '2687');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('196', '25', '5', '5207', '3062', '6359', '3687');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('197', '25', '6', '6163', '3062', '7315', '3687');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('198', '25', '7', '7131', '3083', '8283', '3708');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('199', '25', '2', '4458', '3604', '8317', '5000');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('200', '25', '3', '5449', '4625', '6716', '5500');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('216', '26', '1', '5633', '2145', '6831', '2812');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('217', '26', '2', '4953', '4395', '8629', '5791');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('218', '26', '3', '6394', '5583', '8594', '6520');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('219', '26', '5', '4988', '3770', '6140', '4395');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('220', '26', '6', '6117', '3791', '7269', '4416');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('221', '26', '7', '7281', '3812', '8433', '4437');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('222', '26', '8', '1658', '1833', '2868', '2645');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('223', '26', '9', '990', '3437', '2142', '4062');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('224', '26', '10', '2142', '3458', '3294', '4083');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('225', '26', '11', '3248', '3437', '4400', '4062');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('226', '26', '12', '898', '4062', '4677', '5187');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('227', '26', '13', '2373', '5145', '4389', '5937');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('228', '26', '15', '3640', '5895', '5864', '6645');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('229', '26', '19', '1140', '6208', '4112', '7416');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('230', '26', '21', '1923', '2437', '4205', '3229');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('231', '26', '18', '3732', '6562', '5207', '7500');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('232', '27', '1', '1451', '3500', '2753', '4312');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('233', '27', '3', '3110', '3500', '4654', '4395');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('234', '27', '5', '1785', '4625', '2937', '5250');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('235', '27', '6', '2857', '4666', '4009', '5291');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('236', '27', '7', '3859', '4687', '5011', '5312');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('237', '27', '2', '1071', '5145', '4988', '6041');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('238', '27', '8', '1509', '5750', '2753', '6416');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('239', '27', '13', '3087', '5750', '4838', '6562');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('240', '27', '21', '1774', '6208', '4723', '7020');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('241', '27', '9', '1785', '6854', '2937', '7479');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('242', '27', '10', '2811', '6875', '3963', '7500');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('243', '27', '11', '3836', '6854', '4988', '7479');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('244', '27', '12', '1002', '7270', '4792', '8312');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('245', '27', '19', '4884', '3479', '6923', '4520');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('246', '27', '15', '4884', '4416', '6935', '5291');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('247', '28', '8', '1543', '2458', '2811', '3354');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('248', '28', '9', '1209', '3291', '2361', '3916');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('249', '28', '10', '2304', '3270', '3456', '3895');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('250', '28', '11', '3398', '3270', '4550', '3895');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('251', '28', '12', '668', '3875', '4631', '4770');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('252', '28', '21', '1290', '4541', '4665', '5291');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('253', '28', '13', '3444', '5125', '5046', '5916');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('254', '28', '18', '5368', '7895', '7419', '9062');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('255', '28', '1', '5806', '2520', '6993', '3270');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('256', '28', '5', '5288', '3312', '6440', '3937');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('257', '28', '6', '6347', '3312', '7500', '3937');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('258', '28', '7', '7476', '3291', '8629', '3916');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('259', '28', '2', '4884', '3833', '8755', '5062');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('260', '28', '3', '7534', '4979', '8847', '5791');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('261', '28', '19', '1670', '5708', '5264', '7104');
INSERT INTO `Mall_expresselement` (`Id`, `ExpressId`, `ElementType`, `LeftTopPointX`, `LeftTopPointY`, `RightBottomPointX`, `RightBottomPointY`) VALUES ('262', '28', '15', '1635', '7416', '5334', '8166');



INSERT into Mall_Banner (Shopid,Name,Position,DisplaySequence,Url,Platform,UrlType,STATUS,EnableDelete)
VALUES (0,'限时购',0,-3,'/LimitTimeBuy/home',0,0,1,0);
INSERT into Mall_Banner (Shopid,Name,Position,DisplaySequence,Url,Platform,UrlType,STATUS,EnableDelete)
VALUES (0,'专题',0,-2,'/topic/list',0,0,1,0);
INSERT into Mall_Banner (Shopid,Name,Position,DisplaySequence,Url,Platform,UrlType,STATUS,EnableDelete)
VALUES (0,'积分商城',0,-1,'/IntegralMall',0,0,1,0);

INSERT INTO `Mall_memberintegralrule` VALUES (1, 5, 0);
INSERT INTO `Mall_memberintegralrule` VALUES (2, 6, 0);
INSERT INTO `Mall_memberintegralrule` VALUES (3, 7, 0);
INSERT INTO `Mall_memberintegralrule` VALUES (4, 9, 0);

INSERT INTO `Mall_PlatAccount` VALUES ('1', '0.00', '0.00', '0.00');
INSERT INTO `Mall_ShopAccount` VALUES ('1', '1', '官方旗舰店', '0.00', '0.00', '0.00','');
set foreign_key_checks=1;


INSERT INTO Mall_memberintegralexchangerule(IntegralPerMoney,MoneyPerIntegral) VALUES(0,0);

ALTER TABLE Mall_productcomment
  MODIFY COLUMN AppendContent varchar(1000) CHARACTER
  SET utf8mb4 COLLATE utf8mb4_unicode_ci; 


ALTER TABLE Mall_productcomment
  MODIFY COLUMN ReviewContent varchar(1000) CHARACTER
  SET utf8mb4 COLLATE utf8mb4_unicode_ci;

 ALTER TABLE Mall_member MODIFY COLUMN Nick varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

 ALTER TABLE Mall_shopbonusreceive MODIFY COLUMN WXName varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

 ALTER TABLE Mall_distributor MODIFY COLUMN ShopName varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
