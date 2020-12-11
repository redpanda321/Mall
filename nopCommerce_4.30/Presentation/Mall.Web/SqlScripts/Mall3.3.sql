-- MySQL dump 10.13  Distrib 5.6.23, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: mall33

-- ------------------------------------------------------
--       ******************非凡资源店********************

--       https://item.taobao.com/item.htm?id=582550029912

--       ******************非凡资源店********************
-- ------------------------------------------------------
-- Server version	5.6.25-log

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `mall_account`
--

DROP TABLE IF EXISTS `mall_account`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_account` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `ShopId` BIGINT(20) NOT NULL COMMENT '店铺ID',
  `ShopName` VARCHAR(100) NOT NULL COMMENT '店铺名称',
  `AccountDate` DATETIME NOT NULL COMMENT '出账日期',
  `StartDate` DATETIME NOT NULL COMMENT '开始时间',
  `EndDate` DATETIME NOT NULL COMMENT '结束时间',
  `Status` INT(11) NOT NULL COMMENT '枚举 0未结账，1已结账',
  `ProductActualPaidAmount` DECIMAL(18,2) NOT NULL COMMENT '商品实付总额',
  `FreightAmount` DECIMAL(18,2) NOT NULL COMMENT '运费',
  `CommissionAmount` DECIMAL(18,2) NOT NULL COMMENT '佣金',
  `RefundCommissionAmount` DECIMAL(18,2) NOT NULL COMMENT '退还佣金',
  `RefundAmount` DECIMAL(18,2) NOT NULL COMMENT '退款金额',
  `AdvancePaymentAmount` DECIMAL(18,2) NOT NULL COMMENT '预付款总额',
  `PeriodSettlement` DECIMAL(18,2) NOT NULL COMMENT '本期应结',
  `Remark` TEXT,
  `Brokerage` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '分销佣金',
  `ReturnBrokerage` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '退还分销佣金',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=478 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_account`
--

LOCK TABLES `mall_account` WRITE;
/*!40000 ALTER TABLE `mall_account` DISABLE KEYS */;
INSERT INTO `mall_account` VALUES (157,0,'系统定时任务结算','2017-02-15 00:50:00','2017-02-14 00:00:00','2017-02-15 00:00:00',1,592.20,0.00,0.00,0.00,0.00,0.00,592.20,NULL,0.00,0.00),(158,0,'系统定时任务结算','2017-02-16 00:50:00','2017-02-15 00:00:00','2017-02-16 00:00:00',1,0.00,0.00,0.00,0.00,0.00,0.00,0.00,NULL,0.00,0.00),(159,0,'系统定时任务结算','2017-02-17 00:50:00','2017-02-16 00:00:00','2017-02-17 00:00:00',1,0.00,0.00,0.00,0.00,0.00,0.00,0.00,NULL,0.00,0.00),(160,0,'系统定时任务结算','2017-02-18 00:50:00','2017-02-17 00:00:00','2017-02-18 00:00:00',1,0.00,0.00,0.00,0.00,0.00,0.00,0.00,NULL,0.00,0.00),(161,0,'系统定时任务结算','2017-02-19 00:50:00','2017-02-18 00:00:00','2017-02-19 00:00:00',1,0.00,0.00,0.00,0.00,0.00,0.00,0.00,NULL,0.00,0.00),(162,0,'系统定时任务结算','2017-02-20 09:50:01','2017-02-19 00:00:00','2017-02-20 00:00:00',1,0.00,0.00,0.00,0.00,0.00,0.00,0.00,NULL,0.00,0.00);
/*!40000 ALTER TABLE `mall_account` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_accountdetail`
--

DROP TABLE IF EXISTS `mall_accountdetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_accountdetail` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `AccountId` BIGINT(20) NOT NULL COMMENT '结算记录外键',
  `ShopId` BIGINT(20) NOT NULL COMMENT '店铺ID',
  `ShopName` VARCHAR(100) DEFAULT NULL,
  `Date` DATETIME NOT NULL COMMENT '完成日期',
  `OrderDate` DATETIME NOT NULL COMMENT '订单下单日期',
  `OrderFinshDate` DATETIME NOT NULL,
  `OrderType` INT(11) NOT NULL COMMENT '枚举 完成订单1，退订单0',
  `OrderId` BIGINT(20) NOT NULL COMMENT '订单ID',
  `OrderAmount` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '订单金额',
  `ProductActualPaidAmount` DECIMAL(18,2) NOT NULL COMMENT '商品实付总额',
  `FreightAmount` DECIMAL(18,2) NOT NULL COMMENT '运费金额',
  `TaxAmount` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '税费',
  `IntegralDiscount` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '积分抵扣金额',
  `CommissionAmount` DECIMAL(18,2) NOT NULL COMMENT '佣金',
  `RefundTotalAmount` DECIMAL(18,2) NOT NULL COMMENT '退款金额',
  `RefundCommisAmount` DECIMAL(18,2) NOT NULL COMMENT '退还佣金',
  `OrderRefundsDates` VARCHAR(300) NOT NULL COMMENT '退单的日期集合以;分隔',
  `BrokerageAmount` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '分销佣金',
  `ReturnBrokerageAmount` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '退分销佣金',
  `SettlementAmount` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '结算金额',
  `PaymentTypeName` VARCHAR(100) DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `mall_accountdetails_ibfk_1` (`AccountId`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=1090 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_accountdetail`
--

LOCK TABLES `mall_accountdetail` WRITE;
/*!40000 ALTER TABLE `mall_accountdetail` DISABLE KEYS */;
INSERT INTO `mall_accountdetail` VALUES (532,157,1,'非凡资源店','2017-02-15 00:50:00','0001-01-01 00:00:00','2017-02-14 15:21:30',1,2017021497119677,29.90,29.90,0.00,0.00,0.00,0.00,0.00,0.00,'',0.00,0.00,29.90,'平台线下收款'),(533,157,1,'官方自营店','2017-02-15 00:50:00','0001-01-01 00:00:00','2017-02-14 15:20:57',1,2017021461330622,59.80,59.80,0.00,0.00,0.00,0.00,0.00,0.00,'',0.00,0.00,59.80,'平台线下收款'),(534,157,1,'官方自营店','2017-02-15 00:50:00','0001-01-01 00:00:00','2017-02-14 15:20:42',1,2017021429247711,29.90,29.90,0.00,0.00,0.00,0.00,0.00,0.00,'',0.00,0.00,29.90,'平台线下收款'),(535,157,1,'官方自营店','2017-02-15 00:50:00','0001-01-01 00:00:00','2017-02-14 15:20:09',1,2017021464824161,29.90,29.90,0.00,0.00,0.00,0.00,0.00,0.00,'',0.00,0.00,29.90,'平台线下收款'),(536,157,1,'官方自营店','2017-02-15 00:50:00','0001-01-01 00:00:00','2017-02-14 15:19:52',1,2017021489566321,29.90,29.90,0.00,0.00,0.00,0.00,0.00,0.00,'',0.00,0.00,29.90,'平台线下收款'),(537,157,1,'官方自营店','2017-02-15 00:50:00','0001-01-01 00:00:00','2017-02-14 15:05:46',1,2017021478928561,59.90,59.90,0.00,0.00,0.00,0.00,0.00,0.00,'',0.00,0.00,59.90,'平台线下收款'),(538,157,1,'官方自营店','2017-02-15 00:50:00','0001-01-01 00:00:00','2017-02-14 15:05:31',1,2017021468050488,59.90,59.90,0.00,0.00,0.00,0.00,0.00,0.00,'',0.00,0.00,59.90,'平台线下收款'),(539,157,1,'官方自营店','2017-02-15 00:50:00','0001-01-01 00:00:00','2017-02-14 15:05:16',1,2017021441459359,59.90,59.90,0.00,0.00,0.00,0.00,0.00,0.00,'',0.00,0.00,59.90,'平台线下收款'),(540,157,1,'官方自营店','2017-02-15 00:50:00','0001-01-01 00:00:00','2017-02-14 15:05:00',1,2017021468018578,59.90,59.90,0.00,0.00,0.00,0.00,0.00,0.00,'',0.00,0.00,59.90,'平台线下收款'),(541,157,1,'官方自营店','2017-02-15 00:50:00','0001-01-01 00:00:00','2017-02-14 15:04:39',1,2017021495260570,59.90,59.90,0.00,0.00,0.00,0.00,0.00,0.00,'',0.00,0.00,59.90,'平台线下收款'),(542,157,1,'官方自营店','2017-02-15 00:50:00','0001-01-01 00:00:00','2017-02-14 11:00:42',1,2017021432635793,11.90,11.90,0.00,0.00,0.00,0.00,0.00,0.00,'',0.00,0.00,11.90,'预付款支付'),(543,157,1,'官方自营店','2017-02-15 00:50:00','0001-01-01 00:00:00','2017-02-14 11:00:37',1,2017021495275742,29.90,29.90,0.00,0.00,0.00,0.00,0.00,0.00,'',0.00,0.00,29.90,'预付款支付'),(544,157,1,'官方自营店','2017-02-15 00:50:00','0001-01-01 00:00:00','2017-02-14 11:00:30',1,2017021449033375,49.90,49.90,0.00,0.00,0.00,0.00,0.00,0.00,'',0.00,0.00,49.90,'预付款支付'),(545,157,1,'官方自营店','2017-02-15 00:50:00','0001-01-01 00:00:00','2017-02-14 10:53:20',1,2017021496297002,10.80,10.80,0.00,0.00,0.00,0.00,0.00,0.00,'',0.00,0.00,10.80,'预付款支付'),(546,157,1,'官方自营店','2017-02-15 00:50:00','0001-01-01 00:00:00','2017-02-14 10:36:46',1,2017021424058141,10.80,10.80,0.00,0.00,0.00,0.00,0.00,0.00,'',0.00,0.00,10.80,'预付款支付');
/*!40000 ALTER TABLE `mall_accountdetail` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_accountmeta`
--

DROP TABLE IF EXISTS `mall_accountmeta`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_accountmeta` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `AccountId` BIGINT(20) NOT NULL,
  `MetaKey` VARCHAR(100) NOT NULL,
  `MetaValue` TEXT NOT NULL,
  `ServiceStartTime` DATETIME NOT NULL COMMENT '营销服务开始时间',
  `ServiceEndTime` DATETIME NOT NULL COMMENT '营销服务结束时间',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_accountmeta`
--

LOCK TABLES `mall_accountmeta` WRITE;
/*!40000 ALTER TABLE `mall_accountmeta` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_accountmeta` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_active`
--

DROP TABLE IF EXISTS `mall_active`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_active` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ShopId` BIGINT(20) NOT NULL COMMENT '店铺编号',
  `ActiveName` VARCHAR(200) NOT NULL COMMENT '活动名称',
  `StartTime` DATETIME NOT NULL COMMENT '开始时间',
  `EndTime` DATETIME NOT NULL COMMENT '结束时间',
  `IsAllProduct` TINYINT(1) NOT NULL DEFAULT '1' COMMENT '是否全部商品',
  `IsAllStore` TINYINT(1) NOT NULL DEFAULT '1' COMMENT '是否全部门店',
  `ActiveType` INT(11) NOT NULL COMMENT '活动类型',
  `ActiveStatus` INT(11) NOT NULL DEFAULT '0' COMMENT '活动状态',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `IDX_mall_Active_StartTime` (`StartTime`) USING BTREE,
  KEY `IDX_mall_Active_EndTime` (`EndTime`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=91 DEFAULT CHARSET=utf8 COMMENT='营销活动表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_active`
--

LOCK TABLES `mall_active` WRITE;
/*!40000 ALTER TABLE `mall_active` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_active` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_activemarketservice`
--

DROP TABLE IF EXISTS `mall_activemarketservice`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_activemarketservice` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `TypeId` INT(11) NOT NULL COMMENT '营销服务类型ID',
  `ShopId` BIGINT(20) NOT NULL,
  `ShopName` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=104 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_activemarketservice`
--

LOCK TABLES `mall_activemarketservice` WRITE;
/*!40000 ALTER TABLE `mall_activemarketservice` DISABLE KEYS */;
INSERT INTO `mall_activemarketservice` VALUES (38,5,1,'非凡资源店'),(39,4,1,'非凡资源店'),(40,1,1,'非凡资源店'),(41,2,1,'非凡资源店'),(42,3,1,'非凡资源店');
/*!40000 ALTER TABLE `mall_activemarketservice` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_activeproduct`
--

DROP TABLE IF EXISTS `mall_activeproduct`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_activeproduct` (
  `Id` INT(11) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ActiveId` BIGINT(20) NOT NULL COMMENT '活动编号',
  `ProductId` BIGINT(20) NOT NULL COMMENT '产品编号 -1表示所有商品',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `IDX_mall_Accts_ActiveId` (`ActiveId`) USING BTREE,
  KEY `IDX_mall_Accts_ProdcutId` (`ProductId`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=696 DEFAULT CHARSET=utf8 COMMENT='营销活动商品';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_activeproduct`
--

LOCK TABLES `mall_activeproduct` WRITE;
/*!40000 ALTER TABLE `mall_activeproduct` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_activeproduct` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_agreement`
--

DROP TABLE IF EXISTS `mall_agreement`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_agreement` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `AgreementType` INT(4) NOT NULL COMMENT '协议类型 枚举 AgreementType：0买家注册协议，1卖家入驻协议',
  `AgreementContent` TEXT NOT NULL COMMENT '协议内容',
  `LastUpdateTime` DATETIME NOT NULL COMMENT '最后修改日期',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=5 DEFAULT CHARSET=gbk;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_agreement`
--

LOCK TABLES `mall_agreement` WRITE;
/*!40000 ALTER TABLE `mall_agreement` DISABLE KEYS */;
INSERT INTO `mall_agreement` VALUES (1,0,'<div><p><strong>【审慎阅读】</strong>您在申请注册流程中点击同意前，应当认真阅读以下协议。<strongstyle=\"text-decoration:underline\">请您务必审慎阅读、充分理解协议中相关条款内容，其中包括：</strong></p><p>1、<strongstyle=\"text-decoration:underline\">与您约定免除或限制责任的条款；</strong></p><p>2、<strongstyle=\"text-decoration:underline\">与您约定法律适用和管辖的条款；</strong></p><p>3、<strongstyle=\"text-decoration:underline\">其他以粗体下划线标识的重要条款。</strong></p><p>如您对协议有任何疑问，可向平台客服咨询。</p><p><strong>【特别提示】</strong>当您按照注册页面提示填写信息、阅读并同意协议且完成全部注册程序后，即表示您已充分阅读、理解并接受协议的全部内容。如您因平台服务与mall发生争议的，适用《mall平台服务协议》处理。如您在使用平台服务过程中与其他用户发生争议的，依您与其他用户达成的协议处理。</p><p><strongstyle=\"text-decoration:underline\">阅读协议的过程中，如果您不同意相关协议或其中任何条款约定，您应立即停止注册程序。</strong></p><p><a target=\"_blank\">《mall服务协议》</a></p><p><a target=\"_blank\">《法律声明及隐私权政策》</a></p><p><a target=\"_blank\"></a></p><pstyle=\"text-align:center\"></p></div>','2015-07-15 16:54:35'),(2,1,'<div class=\"board_content\"><p>	为了更好地开拓市场，更好地为本地客户服务，甲乙双方本着互惠互利的双赢策略，根据乙方崇左商城入驻条件，甲方申请入驻乙方商城，并完全接受乙方的规范，经友好协商，甲乙双方签署以下入驻协议。</p><p>	<br>一、&nbsp;入驻商家基本条件<br>1、&nbsp;有良好的合作意愿，能提供优质的商品，保证合作的顺利进行，并保证提供良好的售后服务；<br>2、&nbsp;甲方必须满足以下条件之一（符合其中一项即可）：<br>⑴授权商，获得国际或者国内知名品牌厂商的授权；<br>⑵拥有自己注册商标的生产型厂商；<br>⑶专业品牌经销商，代理商，B2C网站；<br>⑷专业品牌专卖店。<br>3、&nbsp;甲方应在签订本协议时向乙方提供（经乙方认可的）包括但不限于以下证明材料复印件：<br>⑴营业执照 (包括个体户营业执照)、税务登记证；<br>⑵拥有注册商标或者品牌，或者拥有正规的品牌授权书。<br>4、&nbsp;甲方承诺：<br>⑴所有商品一口价销售（参与打折、促销、秒杀活动除外）；<br>⑵七天无损坏不影响二次销售情况下换货(如商品无质量问题，且包装未破坏不影响二次销售，换货费用由买家承担，每张订单只提供一次换货服务；如属商品质量问题，换货费用由商家承担)；<br>⑶实体店地址变更应及时告知乙方；<br>⑷参加乙方全网积分购物活动；<br>⑸所有商品保证原厂正品，保障商品质量、承诺售后服务，必要时能提供销售发票。</p><p>	<br>二、&nbsp;入驻商铺类型<br>1、&nbsp;展示型商铺；<br>2、&nbsp;销售型商铺。</p><p>	&nbsp;</p><p>	三、入驻商铺等级</p><p>	1、普通商铺</p><p>	2、扶持商铺；</p><p>	3、拓展商铺；</p><p>	4、旗舰商铺。</p><p>	<br>三、&nbsp;商铺入驻开通流程<br>1、&nbsp;甲乙双方签署本协议；<br>2、&nbsp;甲方根据所选择商铺类型，提交相应的证明材料复印件；<br>3、&nbsp;在乙方非常商城注册，获取管理员账号；<br>4、&nbsp;乙方审核通过后，甲方即可发布商品并展示经营活动。</p><p>	<br>四、&nbsp;双方权利义务<br>甲方权利义务：<br>1、&nbsp;在本协议第一条规定范围内，甲方自行开拓市场与发展业务，不得以欺诈、胁迫等不正当手段损害客户或甲方的利益与声誉；<br>2、&nbsp;甲方妥善保管商铺管理员账号，如因甲方保管、设置或使用不当造成的经济损失，由甲方自行承担责任；<br>3、&nbsp;甲方在使用过程中如发现任何非法盗用乙方账号出现安全漏洞等情况，应立即通知乙方<br>4、&nbsp;甲方保证其所有经营活动完全符合中国有关法律、法规、行政规章等的规定。如因甲方违反上述规定给乙方带来任何损害，甲方应承担所有法律责任并赔偿由此给甲方造成的损失；<br>5、&nbsp;在协议有效期内，甲方不得向与乙方构成商业竞争关系的企业、商业机构或者组织提供有关乙方相关信息或者资料，否则对乙方造成损失的，由甲方负责赔偿。<br>乙方权利义务：<br>1、&nbsp;乙方对甲方提供必要的开店技术咨询，并保证商城系统平台能正常稳定运行；<br>2、&nbsp;如甲方是托管商铺，乙方应及时为甲方进行店铺更新并实时进行日常管理；<br>3、&nbsp;在本协议有效期限内，乙方有权根据市场情况对各种商铺入驻条件进行调整。</p><p>	<br>五、&nbsp;协议期限<br>本协议有效期为一年，自签订之日起生效。有效期满后，双方可视合作情况续约。</p><p>	六、&nbsp;协议变更、终止及违约责任<br>1、&nbsp;甲乙双方应本着诚实信用的原则履行本协议。任何一方在履约过程中采用欺诈、胁迫或者暴力的手段，另一方均可以解除本协议并要求对方赔偿由此造成的损失；<br>2、&nbsp;在协议执行期间，如果双方或一方认为需要终止，应提前一个月通知对方，双方在财务结算完毕、各自责任明确履行之后，方可终止协议。某方擅自终止本协议，给对方造成损失的，应赔偿对方损失；<br>3、&nbsp;经双方协商达成一致，可以对本协议有关条款进行变更，但应当以书面形式确认；<br>4、&nbsp;一方变更通讯地址或其它联系方式，应自变更之日起十日内，将变更后的地址、联系方式通知另一方，否则变更方应对此造成的一切后果承担责任；<br>5、&nbsp;如因客观情况发生重大变化，致使本协议无继续法履行的，经甲乙双方协商同意，可以变更或者终止协议的履行。</p><p><br>七、&nbsp;保密条款<br>1、甲、乙双方所提供给对方的一切资料、技术和对项目的策划设计要严保密，并只能在合作双方公司的业务范围内使用；<br>2、甲、乙双方均应对在合作过程中所知悉的对的商业和技术秘密承担保密义务。保密条款不受本协议期限的限制。</p><p>	<br>八、&nbsp;争解决<br>在本协议执行期间如果双方发生争议，双方应友好协商解决。如果协商不成，双方同意提交崇左市仲裁委员会进行仲裁，接受其仲裁规则，并服从该仲裁裁决，仲裁费由败诉方承担。争议的解适用中华人民共和国法律、法规、条例和计算机行业惯例。</p></div>','2015-07-15 16:54:35'),(4,2,'<p>mall多用户网上商城系统（B2B2C），是由HiShop推出的一款主要面向中高端客户，为传统企业和大中型网商打造以提高商家运营能力为核心、全面抢占传统互联网＋移动互联网市场，类似京东、天猫的电子商务平台。</p><p><img src=\"/Storage/Plat/APP/About/c9c3442240ba4e898ad9d5dd6df5959d.png\" title=\"a1.png\" alt=\"a1.png\"/></p><p><img src=\"/Storage/Plat/APP/About/4a14c4a6c2ac43299d7385fef352ec2a.png\" title=\"a2.png\" alt=\"a2.png\"/></p><p>采用ASP.NET4.5技术进行分层开发，支持多供应商多店铺经营模式，颠覆了传统的商务模式，通过自营平台与入驻平台共存模式经营。配套微信商城和APP商城，充分挖掘巨大移动电商市场潜力。三大前端数据云同步、线上线下业务充分融合、丰富营销功能引流促销，是各类百货MALL、行业实体市场、电商产业园、行业协会、行业门户网站、各企业等开展平台类型电商业务的首选品牌。</p>','2017-02-16 15:43:11');
/*!40000 ALTER TABLE `mall_agreement` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_appbasesafesetting`
--

DROP TABLE IF EXISTS `mall_appbasesafesetting`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_appbasesafesetting` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `AppKey` VARCHAR(50) NOT NULL,
  `AppSecret` VARCHAR(50) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8 COMMENT='app数据基础安全设置';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_appbasesafesetting`
--

LOCK TABLES `mall_appbasesafesetting` WRITE;
/*!40000 ALTER TABLE `mall_appbasesafesetting` DISABLE KEYS */;
INSERT INTO `mall_appbasesafesetting` VALUES (1,'malltest','has2f5zbd4');
/*!40000 ALTER TABLE `mall_appbasesafesetting` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_applywithdraw`
--

DROP TABLE IF EXISTS `mall_applywithdraw`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_applywithdraw` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `MemId` BIGINT(20) NOT NULL COMMENT '会员ID',
  `NickName` VARCHAR(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `OpenId` VARCHAR(50) DEFAULT NULL COMMENT 'OpenId',
  `ApplyStatus` INT(11) NOT NULL COMMENT '申请状态',
  `ApplyAmount` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '提现金额',
  `ApplyTime` DATETIME NOT NULL COMMENT '申请时间',
  `ConfirmTime` DATETIME DEFAULT NULL COMMENT '处理时间',
  `PayTime` DATETIME DEFAULT NULL COMMENT '付款时间',
  `PayNo` VARCHAR(50) DEFAULT NULL COMMENT '付款流水号',
  `OpUser` VARCHAR(50) DEFAULT NULL COMMENT '操作人',
  `Remark` VARCHAR(200) DEFAULT NULL COMMENT '备注',
  `ApplyType` INT(11) NOT NULL DEFAULT '1' COMMENT '提现方式',
  `Poundage` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '手续费',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_applywithdraw`
--

LOCK TABLES `mall_applywithdraw` WRITE;
/*!40000 ALTER TABLE `mall_applywithdraw` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_applywithdraw` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_appmessage`
--

DROP TABLE IF EXISTS `mall_appmessage`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_appmessage` (
  `Id` INT(20) NOT NULL AUTO_INCREMENT,
  `ShopId` BIGINT(20) NOT NULL DEFAULT '0' COMMENT '商家ID',
  `ShopBranchId` BIGINT(20) NOT NULL DEFAULT '0' COMMENT '门店ID',
  `TypeId` INT(20) NOT NULL COMMENT '消息类型，对应枚举(1=订单，2=售后)',
  `SourceId` BIGINT(20) NOT NULL COMMENT '数据来源编号，对应订单ID或者售后ID',
  `Content` VARCHAR(200) NOT NULL COMMENT '消息内容',
  `IsRead` TINYINT(1) NOT NULL DEFAULT '0' COMMENT '是否已读',
  `sendtime` DATETIME NOT NULL,
  `Title` VARCHAR(50) NOT NULL,
  `OrderPayDate` DATETIME NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=1824 DEFAULT CHARSET=utf8 COMMENT='APP消息通知表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_appmessage`
--

LOCK TABLES `mall_appmessage` WRITE;
/*!40000 ALTER TABLE `mall_appmessage` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_appmessage` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_article`
--

DROP TABLE IF EXISTS `mall_article`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_article` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `CategoryId` BIGINT(20) NOT NULL DEFAULT '0' COMMENT '文章分类ID',
  `Title` VARCHAR(100) NOT NULL COMMENT '文章标题',
  `IconUrl` VARCHAR(100) DEFAULT NULL,
  `Content` MEDIUMTEXT NOT NULL COMMENT '文档内容',
  `AddDate` DATETIME NOT NULL,
  `DisplaySequence` BIGINT(20) NOT NULL,
  `Meta_Title` TEXT COMMENT 'SEO标题',
  `Meta_Description` TEXT COMMENT 'SEO说明',
  `Meta_Keywords` TEXT COMMENT 'SEO关键字',
  `IsRelease` TINYINT(1) NOT NULL COMMENT '是否显示',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_ArticleCategory_Article` (`CategoryId`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=94 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_article`
--

LOCK TABLES `mall_article` WRITE;
/*!40000 ALTER TABLE `mall_article` DISABLE KEYS */;
INSERT INTO `mall_article` VALUES (88,3,'mall 3.1正式发布',NULL,'<p><img src=\"/Storage/Plat/Article/88/8fe82758994049a3a1328851c5c048fc.png\" title=\"image.png\" _src=\"/Storage/Plat/Article/88/8fe82758994049a3a1328851c5c048fc.png\" alt=\"image.png\"/><img src=\"/Storage/Plat/Article/88/673feddd86854b61b4f39ab74b3c18fd.png\" title=\"image.png\" _src=\"/Storage/Plat/Article/88/673feddd86854b61b4f39ab74b3c18fd.png\" alt=\"image.png\"/><img src=\"/Storage/Plat/Article/88/ecb540e5f82645f885718649b1effc0f.png\" title=\"image.png\" _src=\"/Storage/Plat/Article/88/ecb540e5f82645f885718649b1effc0f.png\" alt=\"image.png\"/><img src=\"/Storage/Plat/Article/88/17863d71c7584ea1affc2143c90072d5.png\" title=\"image.png\" _src=\"/Storage/Plat/Article/88/17863d71c7584ea1affc2143c90072d5.png\" alt=\"image.png\"/><strong><img src=\"/Storage/Plat/Article/88/8137bd1b0a3d4ef6bd35f86b8627dca8.png\" title=\"image.png\" _src=\"/Storage/Plat/Article/88/8137bd1b0a3d4ef6bd35f86b8627dca8.png\" alt=\"image.png\"/><img src=\"/Storage/Plat/Article/88/43f241f5ef99457ba0b8ac678ce18812.png\" title=\"image.png\" _src=\"/Storage/Plat/Article/88/43f241f5ef99457ba0b8ac678ce18812.png\" alt=\"image.png\"/></strong></p>','2017-12-28 10:04:49',1,NULL,NULL,NULL,1),(89,3,'商城架构，技术特性',NULL,'<p><img src=\"/Storage/Plat/Article/89/9394f485cb0140b6ba945511d784d3ca.png\" title=\"image.png\" _src=\"/Storage/Plat/Article/89/9394f485cb0140b6ba945511d784d3ca.png\" alt=\"image.png\"/><img src=\"/Storage/Plat/Article/89/507a086cc6ab4ea9824684fa45d5235d.png\" title=\"image.png\" _src=\"/Storage/Plat/Article/89/507a086cc6ab4ea9824684fa45d5235d.png\" alt=\"image.png\"/><img src=\"/Storage/Plat/Article/89/c6369454d8514c3abc6d088e3558f596.png\" title=\"image.png\" _src=\"/Storage/Plat/Article/89/c6369454d8514c3abc6d088e3558f596.png\" alt=\"image.png\"/><img src=\"/Storage/Plat/Article/89/df7e37fe130246ab890f79558b751c05.png\" title=\"image.png\" _src=\"/Storage/Plat/Article/89/df7e37fe130246ab890f79558b751c05.png\" alt=\"image.png\"/><img src=\"/Storage/Plat/Article/89/6c1757fb56cb43f2aea5c63063727147.png\" title=\"image.png\" _src=\"/Storage/Plat/Article/89/6c1757fb56cb43f2aea5c63063727147.png\" alt=\"image.png\"/><img src=\"/Storage/Plat/Article/89/84b8280aa60041c8bb66170c8301947a.png\" title=\"image.png\" _src=\"/Storage/Plat/Article/89/84b8280aa60041c8bb66170c8301947a.png\" alt=\"image.png\"/><img src=\"/Storage/Plat/Article/89/8ed360e2660a4200b07058771d52bd77.png\" title=\"image.png\" _src=\"/Storage/Plat/Article/89/8ed360e2660a4200b07058771d52bd77.png\" alt=\"image.png\"/><img src=\"/Storage/Plat/Article/89/c4fcc395f8354995871b9d8a46498352.png\" title=\"image.png\" _src=\"/Storage/Plat/Article/89/c4fcc395f8354995871b9d8a46498352.png\" alt=\"image.png\"/></p>','2017-12-28 10:05:45',1,NULL,NULL,NULL,1),(90,3,'“决战”双十一，最好用的微商城营销活动都在这了',NULL,'<h2 id=\"activity-name\" style=\"margin: 0px 0px 5px;padding: 0px;font-weight: 400;font-size: 24px;line-height: 1.4\">“决战”双十一，最好用的微商城营销活动都在这了</h2><p><span id=\"post-date\" class=\"rich_media_meta rich_media_meta_text\" style=\"margin: 0px 8px 10px 0px;padding: 0px;display: inline-block;vertical-align: middle;font-size: 14px;color: rgb(153, 153, 153);max-width: none\">2017-10-31</span><span>&nbsp;</span><a class=\"rich_media_meta rich_media_meta_link rich_media_meta_nickname\" href=\"https://mp.weixin.qq.com/s?__biz=MjM5Nzk2OTU4NQ==&mid=2649988947&idx=1&sn=c6b2854b39dbb963f2791718af9aecb2&chksm=bed6d48d89a15d9bfed5ba98aafc0f7f785083dc72eb35688970237c04c0c831913bc97d9dc3&scene=0&key=0760b7ae739a1c26cf8d5422e6ef7b61f0b453a7260de761f35be874c059004b1f669363ed233910d9472725f4a9154ca0fcb2db079a05e5b1caa1f508636440dc4e63550a6b90d41a1e2eadf7244caf&ascene=1&uin=MTQ0MjY0MzU%3D&devicetype=Windows+7&version=6205051a&lang=zh_CN&pass_ticket=%2FKaWKIu0yURWN0VzXwM4WLMBa1ZN5nqNeJ%2FvnRZq2CA%3D&winzoom=1##\" id=\"post-user\" style=\"margin: 0px 8px 10px 0px;padding: 0px;color: rgb(67, 149, 245);vertical-align: middle;font-size: 14px;max-width: none;display: inline-block !important\">体验mall</a></p><blockquote style=\";padding: 0px 0px 0px 10px;max-width: 100%;border-left: 3px solid rgb(219, 219, 219);box-sizing: border-box !important;word-wrap: break-word !important\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;white-space: pre-wrap;text-align: center;box-sizing: border-box !important;word-wrap: break-word !important\"><img data-ratio=\"0.2222222222222222\" data-src=\"http://mmbiz.qpic.cn/mmbiz_gif/e3EOYyI7ibZczyiafTy6zjCpicia6nJLOCqbhicJlzSSGbAC3Yk8ne1XlVKib5N3SzM7dyh79BQO5rq0SKO2dFb6pvBA/0?wx_fmt=gif\" data-type=\"gif\" data-w=\"1080\" width=\"auto\" _width=\"auto\" class=\" __bg_gif\" src=\"https://mmbiz.qpic.cn/mmbiz_gif/e3EOYyI7ibZczyiafTy6zjCpicia6nJLOCqbhicJlzSSGbAC3Yk8ne1XlVKib5N3SzM7dyh79BQO5rq0SKO2dFb6pvBA/0?wx_fmt=gif&tp=webp&wxfrom=5&wx_lazy=1\" data-order=\"0\" data-fail=\"0\" style=\";padding: 0px;height: auto !important;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;white-space: normal;visibility: visible !important;width: auto !important\"/></p><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><section editor=\"bj.96weixin.com\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box;word-wrap: break-word !important\"><section class=\"\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><section editor=\"bj.96weixin.com\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box;word-wrap: break-word !important\"><section class=\"\" style=\";padding: 5px 10px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;border-style: solid;color: rgb(51, 51, 51);border-width: 20px 10px\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">双十一从一开始的淘宝购物狂欢节到现在的全民购物节，已经不限于在淘宝天猫这两个平台，而是全网购物，所有的电商平台都想趁着“双十一“分一杯羹。</span></p></section></section><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">现如今移动互联网的快速发展，促使了更多的国民在移动端购物，双十一80%交易源于移动端就是最好的体现。而微信刚好是移动端的第一流量入口，可以说在”双十一“利用得当，在微信商城的交易绝对不弱于任一个电商平台。</span><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p></section></section></section><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">而想要在微信搭建一个商城，仅靠微信自带的微店肯定是不行的，功能不能满足大多数商家，这时我们就需要找一些第三方商城开发商，例如</span><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px;color: rgb(255, 0, 0)\">mall</span><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">这样的微信第三方服务商，专业做</span><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px;color: rgb(255, 0, 0)\">多用户微信商城系统</span><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">开发。</span><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">淘宝天猫平台汇聚众多的营销活动，比如天天特价、聚划算、限时特价、淘金币等，能够在一定程度上给商家带来巨大的流量。mall多用户微商城系统同样汇聚众多的营销活动，对于新的平台来说，开展商城优惠活动是有效的吸粉引流方式之一。</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">那么多用户微商城的营销优惠活动怎样开展才能达到最好的效果呢?</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></span></p><section class=\"\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;transform: translate3d(1px, 0px, 0px)\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;display: inline-block;text-align: center\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;border-top-width: 2px;border-style: solid solid none;border-color: rgb(239, 112, 96);border-right-width: 2px;border-left-width: 2px;height: 0.8em;border-top-left-radius: 1em;border-top-right-radius: 1em\"></section><section style=\";padding: 0px 10px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;min-width: 2em\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px;white-space: pre-wrap;color: rgb(255, 0, 0)\">一、新人礼包</span></p></section><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;border-right-width: 2px;border-style: none solid solid;border-color: rgb(239, 112, 96);border-bottom-width: 2px;border-left-width: 2px;height: 0.8em;border-bottom-left-radius: 1em;border-bottom-right-radius: 1em\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></section></section></section></section><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">这种销售方法主要是为了鼓励新用户注册，为微商城赢得首批用户。这一方式重点在于培养消费者的购物习惯，用户对于没有下单过的商城一定会有所顾虑，而首次下单优惠无疑会刺激用户的购买。当客户打破了第一次的购买障碍，就会为下次的复购做准备。</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></span></p><section class=\"\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;transform: translate3d(1px, 0px, 0px)\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;display: inline-block;text-align: center\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;border-top-width: 2px;border-style: solid solid none;border-color: rgb(239, 112, 96);border-right-width: 2px;border-left-width: 2px;height: 0.8em;border-top-left-radius: 1em;border-top-right-radius: 1em\"></section><section style=\";padding: 0px 10px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;min-width: 2em\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px;white-space: pre-wrap;color: rgb(255, 0, 0)\">二、组合购优惠</span></p></section><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;border-right-width: 2px;border-style: none solid solid;border-color: rgb(239, 112, 96);border-bottom-width: 2px;border-left-width: 2px;height: 0.8em;border-bottom-left-radius: 1em;border-bottom-right-radius: 1em\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></section></section></section></section><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">组合购优惠主要为了提升买家的购买量，刺激买家购买。重点在于提高用户的客单价。</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">无论是线上还是线下，都在广泛使用满减优惠活动，这一活动的技巧就在于价格的设置，很多用户可能都经历过买两件凑不到满减额度，必须多一件才能符合满减要求，这些都是商家提前做好设计的价格。</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></span></p><section class=\"\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;transform: translate3d(1px, 0px, 0px)\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;display: inline-block;text-align: center\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;border-top-width: 2px;border-style: solid solid none;border-color: rgb(239, 112, 96);border-right-width: 2px;border-left-width: 2px;height: 0.8em;border-top-left-radius: 1em;border-top-right-radius: 1em\"></section><section style=\";padding: 0px 10px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;min-width: 2em\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px;white-space: pre-wrap;color: rgb(255, 0, 0)\">三、节日促销</span></p></section><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;border-right-width: 2px;border-style: none solid solid;border-color: rgb(239, 112, 96);border-bottom-width: 2px;border-left-width: 2px;height: 0.8em;border-bottom-left-radius: 1em;border-bottom-right-radius: 1em\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></section></section></section></section><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">借节日的嘘头来做多用户微商城的微信营销活动。节日促销的重点不在于节日，而是借节日来宣传产品产品，比如刚过去的国庆节，以及未来半个月到来的“双11”节日等。</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></span></p><section class=\"\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;transform: translate3d(1px, 0px, 0px)\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;display: inline-block;text-align: center\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;border-top-width: 2px;border-style: solid solid none;border-color: rgb(239, 112, 96);border-right-width: 2px;border-left-width: 2px;height: 0.8em;border-top-left-radius: 1em;border-top-right-radius: 1em\"></section><section style=\";padding: 0px 10px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;min-width: 2em\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px;white-space: pre-wrap;color: rgb(255, 0, 0)\">四、限时购</span></p></section><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;border-right-width: 2px;border-style: none solid solid;border-color: rgb(239, 112, 96);border-bottom-width: 2px;border-left-width: 2px;height: 0.8em;border-bottom-left-radius: 1em;border-bottom-right-radius: 1em\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></section></section></section></section><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">通过部分产品的限时低价来获取流量，或者通过其它产品来为别的产品的销量做铺垫，这么做的重点在于引流。</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">这种销售方式能很好地吸引用户访问商城。选一两款产品，将价格设置为底价甚至低于底价，利用这些商品吸引用户进店，然后通过其它商品盈利。</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></span></p><section class=\"\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;transform: translate3d(1px, 0px, 0px)\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;display: inline-block;text-align: center\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;border-top-width: 2px;border-style: solid solid none;border-color: rgb(239, 112, 96);border-right-width: 2px;border-left-width: 2px;height: 0.8em;border-top-left-radius: 1em;border-top-right-radius: 1em\"></section><section style=\";padding: 0px 10px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;min-width: 2em\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px;white-space: pre-wrap;color: rgb(255, 0, 0)\">五、佣金推广</span></p></section><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;border-right-width: 2px;border-style: none solid solid;border-color: rgb(239, 112, 96);border-bottom-width: 2px;border-left-width: 2px;height: 0.8em;border-bottom-left-radius: 1em;border-bottom-right-radius: 1em\"></section></section></section></section><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">这也是多用户微商城的分销模式。通过返佣的方式让消费者为店铺做推广。不仅能够扩大店铺的知名度，还能获取新客户。</span><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><section class=\"\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;text-align: center\"><section style=\"margin: 10px 0px;padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;display: inline-block;border-radius: 50px;color: rgb(255, 255, 255);border: 1px solid rgb(117, 117, 118)\"><section style=\";padding: 5px 15px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;display: inline-block;border-radius: 50px;border: 1px solid rgb(117, 117, 118)\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;color: rgb(255, 0, 0);font-size: 14px\">案例</span></p></section></section></section></section><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px;color: rgb(255, 0, 0)\">模式：</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">“<span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\">线上营销+线上销售+线下服务</span>”O2O一站式</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px;color: rgb(255, 0, 0)\">成果：</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">利用mall搭建的点到网作为京粮集团旗下网上商超购物平台，线下商城和线下实体店协作共赢，扩大销量，多种营销方式拉新引流提高复购率。</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;white-space: pre-wrap;box-sizing: border-box !important;word-wrap: break-word !important\"></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;text-align: center;box-sizing: border-box !important;word-wrap: break-word !important\"><br/></p></blockquote><p></p><section class=\"\" data-brushtype=\"text\" data-width=\"100%\" style=\"padding: 0px; max-width: 100%; font-family: 微软雅黑; width: 506px; font-size: 20px; color: rgb(114, 114, 114); text-align: center; font-weight: bold; box-sizing: border-box !important; word-wrap: break-word !important;\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">往期<span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;color: rgb(255, 120, 39)\">精彩</span>回顾</span></section><section data-width=\"100%\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\";padding: 0px 10px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;overflow: hidden;display: inline-block;background-color: rgb(254, 254, 254)\"></section><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;white-space: pre-wrap;text-align: center;box-sizing: border-box !important;word-wrap: break-word !important\"><img class=\"\" data-ratio=\"0.9375\" data-src=\"http://mmbiz.qpic.cn/mmbiz_png/fgnkxfGnnkQJsfHzk0g4uu7Oout4kXavwIic9jfUkqsJ3kVjyFcic0icWTHEnMCv7E8h7ciclOmNpspcnduk5PJia3w/0.png?\" data-w=\"16\" width=\"auto\" _width=\"auto\" src=\"data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQImWNgYGBgAAAABQABh6FO1AAAAABJRU5ErkJggg==\" style=\";padding: 0px;height: 15px !important;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;visibility: visible !important;width: 16px !important\"/></p></section><section class=\"\" powered-by=\"xiumi.us\" style=\";padding: 0px;max-width: 100%;font-family: 微软雅黑;text-align: justify;box-sizing: border-box !important;word-wrap: break-word !important\"><section class=\"\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><section class=\"\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;white-space: pre-wrap;text-align: left;box-sizing: border-box !important;word-wrap: break-word !important\"><a href=\"https://mp.weixin.qq.com/s?__biz=MjM5Nzk2OTU4NQ==&mid=2649988877&idx=1&sn=d4f9038aa8a7b2f155d9ee5d0bb106a4&chksm=bed6d4d389a15dc5673b8fc6ec1eb3bc36d2d7280a96d9c6d1d529e4222884fbfb7da8a67214&scene=21#wechat_redirect\" target=\"_self\" style=\";padding: 0px;color: rgb(67, 149, 245);max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">我们来了！mall多用户商城小程序上线了</span></a></p></section></section></section><section powered-by=\"xiumi.us\" style=\";padding: 0px;max-width: 100%;font-family: 微软雅黑;text-align: justify;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;white-space: pre-wrap;text-align: left;box-sizing: border-box !important;word-wrap: break-word !important\"><a href=\"https://mp.weixin.qq.com/s?__biz=MjM5Nzk2OTU4NQ==&mid=2649988880&idx=1&sn=bc78072003a6ea0670ac628c26cc0aa3&chksm=bed6d4ce89a15dd8115d00c6f47fef6b314f3bc0ccbf4ceee6a58dff328af292ae575a63dda7&scene=21#wechat_redirect\" target=\"_self\" style=\";padding: 0px;color: rgb(67, 149, 245);max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">京东3个月赚了932亿，每天收入10个亿，你还不看好B2B2C模式？</span></a></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;white-space: pre-wrap;text-align: left;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;font-size: 14px;box-sizing: border-box !important;word-wrap: break-word !important\"><a href=\"http://mp.weixin.qq.com/s?__biz=MjM5Nzk2OTU4NQ==&mid=2649988940&idx=1&sn=a5ff0bd5f8bc76bcd64b67daed39bcb3&scene=21#wechat_redirect\" target=\"_blank\" style=\";padding: 0px;color: rgb(67, 149, 245);max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\">这份拉新营销活动清单，让你双11分分钟爆单！</a></span></p></section></section></section><p></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p></p><section class=\"\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><section class=\"\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br/></p></section></section>','2017-12-28 16:21:25',1,NULL,NULL,NULL,1),(91,3,'饿了么跑马圈地后，外卖O2O平台下个拐点在哪儿？',NULL,'<h2 id=\"activity-name\" style=\"margin: 0px 0px 5px;padding: 0px;font-weight: 400;font-size: 24px;line-height: 1.4\">饿了么跑马圈地后，外卖O2O平台下个拐点在哪儿？</h2><p><span id=\"post-date\" class=\"rich_media_meta rich_media_meta_text\" style=\"margin: 0px 8px 10px 0px;padding: 0px;display: inline-block;vertical-align: middle;font-size: 14px;color: rgb(153, 153, 153);max-width: none\">2017-10-20</span><span>&nbsp;</span><a class=\"rich_media_meta rich_media_meta_link rich_media_meta_nickname\" href=\"https://mp.weixin.qq.com/s?__biz=MjM5Nzk2OTU4NQ==&mid=2649988936&idx=1&sn=6b060f388bbe78c4adddde532d982a28&chksm=bed6d49689a15d8019e1e82aac75c2ff647869fa53c4e1bf824d5ecf538db027467ac82dede6&scene=0&key=0760b7ae739a1c26a685bde08b215a394286f0150a36308307f87bddac1364c4542309fc46370624d65261e7cf4d66fdd4fcf5e25acb927d28f44dc2b7fa091e4e6081056fa6be0eb9ad012c5eb35e6f&ascene=1&uin=MTQ0MjY0MzU%3D&devicetype=Windows+7&version=6205051a&lang=zh_CN&pass_ticket=%2FKaWKIu0yURWN0VzXwM4WLMBa1ZN5nqNeJ%2FvnRZq2CA%3D&winzoom=1##\" id=\"post-user\" style=\"margin: 0px 8px 10px 0px;padding: 0px;color: rgb(67, 149, 245);vertical-align: middle;font-size: 14px;max-width: none;display: inline-block !important\">体验mall</a><section class=\"\" data-id=\"85956\" style=\";padding: 0px;max-width: 100%;border: 0px none;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;height: 6px\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;height: 6px;width: 6px;float: right;border: 1px solid rgb(128, 177, 53)\"></section></section><section class=\"\" style=\";padding: 1em;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;border: 1px solid rgb(128, 177, 53)\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">懒人经济的袭来，催生了一批各行业上门服务的公司。餐饮界，像美团外卖、饿了么、百度外卖等外卖O2O公司迅速兴起并覆盖至全国，消费者足不出户就能享受到各种美食。随着产品体验的升级，用户更加依赖于手边的服务，且对其要求越来越高。外卖O2O行业已到红海阶段。外卖O2O现在如何？如何才能进入外卖O2O行业？下面就和小编一起来了解下吧。</span></p></section><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;height: 6px;color: inherit\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;height: 6px;width: 6px;float: left;border: 1px solid rgb(128, 177, 53)\"></section><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;height: 6px;width: 6px;float: right;border: 1px solid rgb(128, 177, 53)\"></section></section><section data-filtered=\"filtered\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;width: 0px;height: 0px;clear: both\"></section></section></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;white-space: pre-wrap;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p><section class=\"\" data-id=\"85964\" style=\";padding: 0px;max-width: 100%;border: 0px none;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\"margin: 0px 2.1em;padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;height: 0px;vertical-align: middle;border-top-width: 1.1em;border-top-style: solid;border-top-color: rgb(194, 201, 42);border-bottom-width: 1.1em;border-bottom-style: solid;border-bottom-color: rgb(194, 201, 42);border-left-width: 1.1em !important;border-left-style: solid !important;border-left-color: transparent !important;border-right-width: 1.1em !important;border-right-style: solid !important;border-right-color: transparent !important\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;text-align: center;color: rgb(194, 201, 42)\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;white-space: pre-wrap;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;color: rgb(255, 255, 255)\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;line-height: 25.6px\">外卖O2O现状，未来前景如何</span><strong style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\">&nbsp;</strong></span></p></section></section><section data-filtered=\"filtered\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;width: 0px;height: 0px;clear: both\"></section></section></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">据中国外卖大数据显示，2016年，中国餐饮行业市场规模为3.6万亿元，年增速10.8%，预计到2020年，市场规模可突破5万亿元。</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">如果仅仅把外卖看成一个送餐的平台，他们其实并不是特别大，但是如果在送餐之外，还有朋友聚会、家庭的日常用品消费等，格局就会大些。但是如果是包括各个商家、消费者、公司、家庭、日常生活用品、生鲜、商超等都由它来供应和连接的话，这就是一个巨大的市场了。尤其是近两年美团外卖、饿了么等订餐平台在消费者日常生活中强烈的存在感，让一大批创业者跃跃欲试。那么如何才能抓住外卖O2O风口？</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">连接外卖与用户之间的纽带才是未来的关键，如何连接这样的一个关系呢？</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p><section class=\"\" data-id=\"85964\" style=\";padding: 0px;max-width: 100%;border: 0px none;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\"margin: 0px 2.1em;padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;height: 0px;vertical-align: middle;border-top-width: 1.1em;border-top-style: solid;border-top-color: rgb(194, 201, 42);border-bottom-width: 1.1em;border-bottom-style: solid;border-bottom-color: rgb(194, 201, 42);border-left-width: 1.1em !important;border-left-style: solid !important;border-left-color: transparent !important;border-right-width: 1.1em !important;border-right-style: solid !important;border-right-color: transparent !important\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;text-align: center;color: rgb(194, 201, 42)\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;white-space: pre-wrap;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;color: rgb(255, 255, 255)\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;line-height: 25.6px\">搭建类似美团、饿了么的平台</span><strong class=\"\" data-brushtype=\"text\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"></strong><strong style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\">&nbsp;</strong></span></p></section></section><section data-filtered=\"filtered\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;width: 0px;height: 0px;clear: both\"></section></section></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">想要进入外卖O2O行业，就必须要搭建一个O2O的外卖平台。国内外卖市场发展到现在，基本形成了以饿了么和美团居于第一阵营，百度外卖第二阵营，其它外卖项目垫底的市场格局。想要跟他们竞争并打开一条不一样的营销通道至关重要，什么样的系统才能满足外卖O2O市场强大冲击已经用户苛刻的要求呢？</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><img data-s=\"300,640\" data-type=\"jpeg\" data-src=\"https://mmbiz.qpic.cn/mmbiz_jpg/P5D9vJUBHuINicC638secH7QQJ9nLf1ddyLxkicEtCXYzPCYwj3E7PicHbpiaqzaRNPcHBCWiaH6PZ10pCcJ04NJv9A/0?wx_fmt=jpeg\" data-copyright=\"0\" class=\"\" data-ratio=\"0.5576208178438662\" data-w=\"538\" src=\"https://mmbiz.qpic.cn/mmbiz_jpg/P5D9vJUBHuINicC638secH7QQJ9nLf1ddyLxkicEtCXYzPCYwj3E7PicHbpiaqzaRNPcHBCWiaH6PZ10pCcJ04NJv9A/640?wx_fmt=jpeg&tp=webp&wxfrom=5&wx_lazy=1\" data-fail=\"0\" style=\";padding: 0px;height: auto !important;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;width: auto !important;visibility: visible !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p><section class=\"\" data-id=\"85964\" style=\";padding: 0px;max-width: 100%;border: 0px none;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\"margin: 0px 2.1em;padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;height: 0px;vertical-align: middle;border-top-width: 1.1em;border-top-style: solid;border-top-color: rgb(194, 201, 42);border-bottom-width: 1.1em;border-bottom-style: solid;border-bottom-color: rgb(194, 201, 42);border-left-width: 1.1em !important;border-left-style: solid !important;border-left-color: transparent !important;border-right-width: 1.1em !important;border-right-style: solid !important;border-right-color: transparent !important\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;text-align: center;color: rgb(194, 201, 42)\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;white-space: pre-wrap;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;color: rgb(255, 255, 255)\"><strong class=\"\" data-brushtype=\"text\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;line-height: 25.6px\">mall外卖O2O系统来袭</span></strong><strong style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\">&nbsp;</strong></span></p></section></section><section data-filtered=\"filtered\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;width: 0px;height: 0px;clear: both\"></section></section></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">目前餐饮市场巨大的行业前景，以及社会化发展的需求，未来外卖O2O必然成为城市人群消费的主流。一款能够应对餐饮市场冲击，针对不同用户搭建不同外卖平台的系统才能够让企业能够轻松运营。企业可以选择mall O2O外卖系统，创建一个属于当地的O2O外卖平台，既保证了平台系统足够强大，也能够助力企业后期良性发展营收。</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">利用mall搭建的O2O外卖平台，能够为当地用户提供：餐饮、外卖、配送、商超、生鲜、水果等各种便民服务。多元化的供需市场，让本地外卖平台在提供快捷消费方式的同时，也能改变人们的消费习惯，吸引当地更多的商家入驻平台，实现多方合作共赢。</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p><section class=\"\" data-id=\"85964\" style=\";padding: 0px;max-width: 100%;border: 0px none;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\"margin: 0px 2.1em;padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;height: 0px;vertical-align: middle;border-top-width: 1.1em;border-top-style: solid;border-top-color: rgb(194, 201, 42);border-bottom-width: 1.1em;border-bottom-style: solid;border-bottom-color: rgb(194, 201, 42);border-left-width: 1.1em !important;border-left-style: solid !important;border-left-color: transparent !important;border-right-width: 1.1em !important;border-right-style: solid !important;border-right-color: transparent !important\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;text-align: center;color: rgb(194, 201, 42)\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;white-space: pre-wrap;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;color: rgb(255, 255, 255)\"><strong style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\">&nbsp;<strong class=\"\" data-brushtype=\"text\" style=\";padding: 0px;max-width: 100%;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;line-height: 25.6px\">mall外卖O2O系统</span></strong></strong><strong class=\"\" data-brushtype=\"text\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;line-height: 25.6px\">核心功能</span></strong></span></p></section></section><section data-filtered=\"filtered\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;width: 0px;height: 0px;clear: both\"></section></section></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;white-space: pre-wrap;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">1、自定义分类：外卖、水果、蛋糕、便利店，根据不同的商品进行整合分类，让用户更加准确的查看到信息。</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br data-filtered=\"filtered\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">2、便捷下单：系统不仅APP+微信全面覆盖，而且还支持支付宝、网银等多种平台支付方式，让外卖更加便捷。</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br data-filtered=\"filtered\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">3、商家移动化管理工具：便捷的操作，详细的数据统计，商家可通过手机查看下单、抢购、发货等情况，真正做到足不出户就可以掌握全局。</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br data-filtered=\"filtered\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">4、两种配送方式：支持“门店配送”和“到店自提”两种配送方式，可在后台自由配置用哪种方式，满足消费者不同需求。</span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p><section class=\"\" data-brushtype=\"text\" data-width=\"100%\" style=\";padding: 0px;max-width: 100%;font-family: 微软雅黑;width: 506px;font-size: 20px;color: rgb(114, 114, 114);text-align: center;font-weight: bold;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">往期<span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;color: rgb(255, 120, 39)\">精彩</span>回顾</span></section><section data-width=\"100%\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\";padding: 0px 10px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;overflow: hidden;display: inline-block;background-color: rgb(254, 254, 254)\"></section><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;white-space: pre-wrap;text-align: center;box-sizing: border-box !important;word-wrap: break-word !important\"><img class=\"\" data-ratio=\"0.9375\" data-src=\"http://mmbiz.qpic.cn/mmbiz_png/fgnkxfGnnkQJsfHzk0g4uu7Oout4kXavwIic9jfUkqsJ3kVjyFcic0icWTHEnMCv7E8h7ciclOmNpspcnduk5PJia3w/0.png?\" data-w=\"16\" width=\"auto\" _width=\"auto\" src=\"data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQImWNgYGBgAAAABQABh6FO1AAAAABJRU5ErkJggg==\" style=\";padding: 0px;height: 15px !important;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;visibility: visible !important;width: 16px !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;white-space: pre-wrap;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p></section><section class=\"\" powered-by=\"xiumi.us\" style=\";padding: 0px;max-width: 100%;font-family: 微软雅黑;text-align: justify;box-sizing: border-box !important;word-wrap: break-word !important\"><section class=\"\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><section class=\"\" style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;white-space: pre-wrap;text-align: left;box-sizing: border-box !important;word-wrap: break-word !important\"><a href=\"https://mp.weixin.qq.com/s?__biz=MjM5Nzk2OTU4NQ==&mid=2649988877&idx=1&sn=d4f9038aa8a7b2f155d9ee5d0bb106a4&chksm=bed6d4d389a15dc5673b8fc6ec1eb3bc36d2d7280a96d9c6d1d529e4222884fbfb7da8a67214&scene=21#wechat_redirect\" target=\"_self\" style=\";padding: 0px;color: rgb(67, 149, 245);max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">我们来了！mall多用户商城小程序上线了</span></a></p></section></section></section><section powered-by=\"xiumi.us\" style=\";padding: 0px;max-width: 100%;font-family: 微软雅黑;text-align: justify;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;white-space: pre-wrap;text-align: left;box-sizing: border-box !important;word-wrap: break-word !important\"><a href=\"https://mp.weixin.qq.com/s?__biz=MjM5Nzk2OTU4NQ==&mid=2649988880&idx=1&sn=bc78072003a6ea0670ac628c26cc0aa3&chksm=bed6d4ce89a15dd8115d00c6f47fef6b314f3bc0ccbf4ceee6a58dff328af292ae575a63dda7&scene=21#wechat_redirect\" target=\"_self\" style=\";padding: 0px;color: rgb(67, 149, 245);max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;font-size: 14px\">京东3个月赚了932亿，每天收入10个亿，你还不看好B2B2C模式？</span></a></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;white-space: pre-wrap;text-align: left;box-sizing: border-box !important;word-wrap: break-word !important\"><span style=\";padding: 0px;max-width: 100%;font-size: 14px;box-sizing: border-box !important;word-wrap: break-word !important\"><a href=\"http://mp.weixin.qq.com/s?__biz=MjM5Nzk2OTU4NQ==&mid=2649988928&idx=1&sn=6cd0ba9082f4778a502a5b12fa3aafb2&scene=21#wechat_redirect\" target=\"_blank\" style=\";padding: 0px;color: rgb(67, 149, 245);max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\">转化率超过APP的小程序，设计原则只有一个字！</a></span></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;white-space: pre-wrap;text-align: left;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p></section></section></section><section style=\";padding: 0px;max-width: 100%;font-family: 微软雅黑;text-align: justify;line-height: 25.6px;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\"margin: 2em 0px 0px;padding: 0.5em 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;border-style: solid none none;color: rgb(166, 166, 166);line-height: 22.4px;font-family: inherit;font-size: 1em;font-weight: inherit;border-top-color: rgb(204, 204, 204);border-top-width: 1px\"><section style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;border: currentcolor;text-align: center;line-height: 1.4\"><span style=\";padding: 8px 23px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;border-color: rgb(30, 155, 232);border-radius: 25px;color: rgb(255, 255, 255);font-family: inherit;font-weight: inherit;background-color: rgb(30, 155, 232)\">关注体验mall</span></section></section></section></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;font-family: 微软雅黑;text-align: justify;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p><section class=\"\" data-tools=\"135编辑器\" data-id=\"85994\" style=\";padding: 0px;max-width: 100%;font-family: 微软雅黑;text-align: justify;line-height: 25.6px;border: 0px none;box-sizing: border-box !important;word-wrap: break-word !important\"><section style=\"margin: 10px auto;padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important;text-align: center\"><br/></section></section></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;box-sizing: border-box !important;word-wrap: break-word !important\"><br style=\";padding: 0px;max-width: 100%;box-sizing: border-box !important;word-wrap: break-word !important\"/></p><p style=\"margin-top: 0px;margin-bottom: 0px;padding: 0px;max-width: 100%;clear: both;min-height: 1em;font-size: 16px;font-family: 微软雅黑;text-align: justify;box-sizing: border-box !important;word-wrap: break-word !important\"><br/></p>','2017-12-28 16:37:50',1,NULL,NULL,NULL,1),(92,3,'mall 3.2 正式发布',NULL,'<p><img src=\"/Storage/Plat/Article/87/80b3cdb4f89e4b23bb5bdd70468ba8d5.jpg\" title=\"1920x1080.jpg\" _src=\"/Storage/Plat/Article/87/80b3cdb4f89e4b23bb5bdd70468ba8d5.jpg\" alt=\"1920x1080.jpg\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px; white-space: normal;\"/></p>','2018-01-04 12:00:13',1,NULL,NULL,NULL,1);
/*!40000 ALTER TABLE `mall_article` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_articlecategory`
--

DROP TABLE IF EXISTS `mall_articlecategory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_articlecategory` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `ParentCategoryId` BIGINT(20) NOT NULL,
  `Name` VARCHAR(100) DEFAULT NULL COMMENT '文章类型名称',
  `DisplaySequence` BIGINT(20) NOT NULL COMMENT '显示顺序',
  `IsDefault` TINYINT(1) NOT NULL COMMENT '是否为默认',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=37 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_articlecategory`
--

LOCK TABLES `mall_articlecategory` WRITE;
/*!40000 ALTER TABLE `mall_articlecategory` DISABLE KEYS */;
INSERT INTO `mall_articlecategory` VALUES (1,0,'底部帮助',1,1),(2,0,'系统快报',2,1),(3,0,'商城公告',3,1),(4,0,'商家后台公告',4,1),(9,1,'购物指南',1,0),(10,1,'店主之家',1,0),(11,1,'支付方式',1,0),(12,1,'售后服务',1,0),(13,1,'关于我们',1,0),(14,0,'保障服务',1,1),(15,14,'七天无理由',1,1),(16,14,'消费者保障',1,1),(17,14,'及时发货服',1,1);
/*!40000 ALTER TABLE `mall_articlecategory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_attribute`
--

DROP TABLE IF EXISTS `mall_attribute`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_attribute` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `TypeId` BIGINT(20) NOT NULL,
  `Name` VARCHAR(100) NOT NULL COMMENT '名称',
  `DisplaySequence` BIGINT(20) NOT NULL,
  `IsMust` TINYINT(1) NOT NULL COMMENT '是否为必选',
  `IsMulti` TINYINT(1) NOT NULL COMMENT '是否可多选',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Type_Attribute` (`TypeId`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=209 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_attribute`
--

LOCK TABLES `mall_attribute` WRITE;
/*!40000 ALTER TABLE `mall_attribute` DISABLE KEYS */;
INSERT INTO `mall_attribute` VALUES (192,82,'坚果',0,0,0),(193,82,'大小',0,0,0),(194,83,'女装',0,0,0),(195,84,'屏幕尺寸',0,0,0),(196,85,'价格',0,0,0),(197,85,'原产地',0,0,0),(198,85,'分类',0,0,0),(199,87,'性别',0,0,0),(200,87,'功效',0,0,0),(201,93,'适用类型',0,0,0),(202,93,'药品类型',0,0,0),(203,93,'适应人群',0,0,0),(204,93,'剂型',0,0,0);
/*!40000 ALTER TABLE `mall_attribute` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_attributevalue`
--

DROP TABLE IF EXISTS `mall_attributevalue`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_attributevalue` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `AttributeId` BIGINT(20) NOT NULL COMMENT '属性ID',
  `Value` VARCHAR(100) NOT NULL COMMENT '属性值',
  `DisplaySequence` BIGINT(20) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Attribute_AttributeValue` (`AttributeId`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=858 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_attributevalue`
--

LOCK TABLES `mall_attributevalue` WRITE;
/*!40000 ALTER TABLE `mall_attributevalue` DISABLE KEYS */;
INSERT INTO `mall_attributevalue` VALUES (810,192,'核桃',1),(811,192,'松子',1),(812,192,'碧根果',1),(813,193,'1包装',1),(814,193,'2包装',1),(815,193,'5包装',1),(816,194,'衬衣',1),(817,194,'裙子',1),(818,194,'连衣裙',1),(819,194,'外套',1),(820,194,'大衣',1),(821,195,'12.5英寸',1),(822,195,'13.3英寸',1),(823,195,'15.6英寸',1),(824,195,'其他',1),(825,196,'0-49',1),(826,196,'50-99',1),(827,196,'100-149',1),(828,196,'150-199',1),(829,197,'加拿大',1),(830,197,'厄瓜多尔',1),(831,197,'阿根廷',1),(832,197,'中国大陆',1),(833,197,'其它\r\n\r\n',1),(834,198,'甜虾',1),(835,198,'白虾',1),(836,199,'男',1),(837,199,'女',1),(838,200,'补水',1),(839,200,'保湿',1),(840,201,'体虚感冒',1),(841,201,'普通感冒',1),(842,201,'风热感冒',1),(843,202,'西药',1),(844,202,'中药',1),(845,203,'老人',1),(846,203,'儿童',1),(847,203,'承认',1),(848,203,'不限',1),(849,204,'颗粒',1),(850,204,'片剂',1),(851,204,'胶囊',1),(852,204,'口服液',1);
/*!40000 ALTER TABLE `mall_attributevalue` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_autoreply`
--

DROP TABLE IF EXISTS `mall_autoreply`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_autoreply` (
  `Id` INT(11) NOT NULL AUTO_INCREMENT,
  `RuleName` VARCHAR(50) DEFAULT NULL COMMENT '规则名称',
  `Keyword` VARCHAR(30) DEFAULT NULL COMMENT '关键词',
  `MatchType` INT(11) NOT NULL COMMENT '匹配方式(模糊，完全匹配)',
  `TextReply` VARCHAR(300) DEFAULT NULL COMMENT '文字回复内容',
  `IsOpen` INT(11) NOT NULL DEFAULT '0' COMMENT '是否开启',
  `ReplyType` INT(11) NOT NULL COMMENT '消息回复类型-(关注回复，关键词回复，消息自动回复)',
  `ReplyContentType` INT(11) NOT NULL DEFAULT '1' COMMENT '消息内容的类型，1=文本，2=图文素材',
  `MediaId` VARCHAR(200) DEFAULT NULL COMMENT '素材ID',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_autoreply`
--

LOCK TABLES `mall_autoreply` WRITE;
/*!40000 ALTER TABLE `mall_autoreply` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_autoreply` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_banner`
--

DROP TABLE IF EXISTS `mall_banner`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_banner` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `ShopId` BIGINT(20) NOT NULL,
  `Name` VARCHAR(100) NOT NULL COMMENT '导航名称',
  `Position` INT(11) NOT NULL COMMENT '导航显示位置',
  `DisplaySequence` BIGINT(20) NOT NULL,
  `Url` VARCHAR(1000) NOT NULL COMMENT '跳转URL',
  `Platform` INT(11) NOT NULL DEFAULT '0' COMMENT '显示在哪个终端',
  `UrlType` INT(11) NOT NULL DEFAULT '0',
  `STATUS` INT(11) NOT NULL DEFAULT '1' COMMENT '开启或者关闭',
  `EnableDelete` INT(11) NOT NULL DEFAULT '1' COMMENT '能否删除',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=82 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_banner`
--

LOCK TABLES `mall_banner` WRITE;
/*!40000 ALTER TABLE `mall_banner` DISABLE KEYS */;
INSERT INTO `mall_banner` VALUES (73,0,'限时购',0,-3,'/LimitTimeBuy/home',0,0,1,0),(74,0,'专题',0,-2,'/topic/list',0,0,1,0),(75,0,'积分商城',0,-1,'/IntegralMall',0,0,1,0),(76,1,'首页',0,1,'./Shop/Home/1',0,0,1,0),(77,1,'服装',0,2,'./shop/SearchAd?cid=351&sid=1&pageNo=1',0,0,1,0),(78,1,'食品',0,3,'./Shop/SearchAd?cid=352&sid=1&pageNo=1',0,0,1,0),(79,1,'生鲜',0,4,'./Shop/SearchAd?cid=355&sid=1&pageNo=1',0,0,1,0),(80,1,'电脑',0,5,'./Shop/SearchAd?cid=357&sid=1&pageNo=1',0,0,1,0),(81,1,'个护',0,6,'./Shop/SearchAd?cid=359&sid=1&pageNo=1',0,0,1,0);
/*!40000 ALTER TABLE `mall_banner` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_bonus`
--

DROP TABLE IF EXISTS `mall_bonus`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_bonus` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `Type` INT(11) NOT NULL COMMENT '类型，活动红包，关注送红包',
  `Style` INT(11) NOT NULL COMMENT '样式，模板一（源生风格），模板二',
  `Name` VARCHAR(100) DEFAULT NULL COMMENT '名称',
  `MerchantsName` VARCHAR(50) DEFAULT NULL COMMENT '商户名称',
  `Remark` VARCHAR(200) DEFAULT NULL COMMENT '备注',
  `Blessing` VARCHAR(100) DEFAULT NULL COMMENT '祝福语',
  `TotalPrice` DECIMAL(18,2) NOT NULL COMMENT '总面额',
  `StartTime` DATETIME NOT NULL COMMENT '开始日期',
  `EndTime` DATETIME NOT NULL COMMENT '结束日期',
  `QRPath` VARCHAR(100) DEFAULT NULL COMMENT '二维码',
  `PriceType` INT(11) NOT NULL COMMENT '是否固定金额',
  `FixedAmount` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '固定金额',
  `RandomAmountStart` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '随机金额起止范围',
  `RandomAmountEnd` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '随机金额起止范围',
  `ReceiveCount` INT(11) NOT NULL,
  `ImagePath` VARCHAR(100) DEFAULT NULL,
  `Description` VARCHAR(255) DEFAULT NULL,
  `IsInvalid` TINYINT(1) NOT NULL,
  `ReceivePrice` DECIMAL(18,2) NOT NULL,
  `ReceiveHref` VARCHAR(200) NOT NULL,
  `IsAttention` TINYINT(1) NOT NULL,
  `IsGuideShare` TINYINT(1) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_bonus`
--

LOCK TABLES `mall_bonus` WRITE;
/*!40000 ALTER TABLE `mall_bonus` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_bonus` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_bonusreceive`
--

DROP TABLE IF EXISTS `mall_bonusreceive`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_bonusreceive` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `BonusId` BIGINT(20) NOT NULL COMMENT '红包Id',
  `OpenId` VARCHAR(100) DEFAULT NULL COMMENT '领取人微信Id',
  `ReceiveTime` DATETIME DEFAULT NULL COMMENT '领取日期',
  `Price` DECIMAL(18,2) NOT NULL COMMENT '领取金额',
  `IsShare` TINYINT(1) NOT NULL,
  `IsTransformedDeposit` TINYINT(1) NOT NULL COMMENT '红包金额是否已经转入了预存款',
  `UserId` BIGINT(20) DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Reference_1` (`BonusId`) USING BTREE,
  KEY `FK_UserId` (`UserId`) USING BTREE
) ENGINE=INNODB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_bonusreceive`
--

LOCK TABLES `mall_bonusreceive` WRITE;
/*!40000 ALTER TABLE `mall_bonusreceive` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_bonusreceive` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_brand`
--

DROP TABLE IF EXISTS `mall_brand`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_brand` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(100) NOT NULL COMMENT '品牌名称',
  `DisplaySequence` BIGINT(20) NOT NULL COMMENT '顺序',
  `Logo` VARCHAR(1000) DEFAULT NULL COMMENT 'LOGO',
  `RewriteName` VARCHAR(50) DEFAULT NULL COMMENT '未使用',
  `Description` VARCHAR(1000) DEFAULT NULL COMMENT '说明',
  `Meta_Title` VARCHAR(1000) DEFAULT NULL COMMENT 'SEO',
  `Meta_Description` VARCHAR(1000) DEFAULT NULL,
  `Meta_Keywords` VARCHAR(1000) DEFAULT NULL,
  `IsRecommend` TINYINT(1) NOT NULL,
  `IsDeleted` BIT(1) NOT NULL COMMENT '是否已删除',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `Id` (`Id`),
  KEY `Id_2` (`Id`)
) ENGINE=INNODB AUTO_INCREMENT=370 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_brand`
--

LOCK TABLES `mall_brand` WRITE;
/*!40000 ALTER TABLE `mall_brand` DISABLE KEYS */;
INSERT INTO `mall_brand` VALUES (319,'三只松鼠',0,'/Storage/Plat/Brand/logo_319.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(320,'卫龙',0,'/Storage/Plat/Brand/logo_320.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(321,'only',0,'/Storage/Plat/Brand/logo_321.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(322,'苹果',0,'/Storage/Plat/Brand/logo_322.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(323,'大洋世家',0,'/Storage/Plat/Brand/logo_323.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(324,'欧莱雅',0,'/Storage/Plat/Brand/logo_324.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(325,'美的',0,'/Storage/Plat/Brand/logo_325.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(326,'海尔',0,'/Storage/Plat/Brand/logo_326.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(327,'飞利浦',0,'/Storage/Plat/Brand/logo_327.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(328,'贝德玛',0,'/Storage/Plat/Brand/logo_328.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(329,'小米',0,'/Storage/Plat/Brand/logo_329.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(330,'魅族',0,'/Storage/Plat/Brand/logo_330.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(331,'索尼',0,'/Storage/Plat/Brand/logo_331.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(332,'Intel',0,'/Storage/Plat/Brand/logo_332.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(333,'戴尔',0,'/Storage/Plat/Brand/logo_333.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(334,'联想',0,'/Storage/Plat/Brand/logo_334.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(335,'松下',0,'/Storage/Plat/Brand/logo_335.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(336,'喜临门',0,'/Storage/Plat/Brand/logo_336.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(337,'Aimer',0,'/Storage/Plat/Brand/logo_337.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(338,'GXG',0,'/Storage/Plat/Brand/logo_338.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(339,'杰克琼斯',0,'/Storage/Plat/Brand/logo_339.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(340,'威古力',0,'/Storage/Plat/Brand/logo_340.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(341,'百雀羚',0,'/Storage/Plat/Brand/logo_341.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(342,'花王',0,'/Storage/Plat/Brand/logo_342.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(343,'蓝月亮',0,'/Storage/Plat/Brand/logo_343.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(344,'百丽',0,'/Storage/Plat/Brand/logo_344.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(345,'红蜻蜓',0,'/Storage/Plat/Brand/logo_345.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(346,'匡威',0,'/Storage/Plat/Brand/logo_346.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(347,'探路者',0,'/Storage/Plat/Brand/logo_347.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(348,'善存',0,'/Storage/Plat/Brand/logo_348.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(349,'良品铺子',0,'/Storage/Plat/Brand/logo_349.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(350,'养生堂',0,'/Storage/Plat/Brand/logo_350.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(351,'sr',0,'/Storage/Plat/Brand/logo_351.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(352,'YONEX',0,'/Storage/Plat/Brand/logo_352.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(353,'耐克',0,'/Storage/Plat/Brand/logo_353.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(354,'NEW',0,'/Storage/Plat/Brand/logo_354.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(355,'周大福',0,'/Storage/Plat/Brand/logo_355.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(356,'周生生',0,'/Storage/Plat/Brand/logo_356.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(357,'莱百首饰',0,'/Storage/Plat/Brand/logo_357.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(358,'珂莱钻石',0,'/Storage/Plat/Brand/logo_358.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(359,'Cartier',0,'/Storage/Plat/Brand/logo_359.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(360,'Tiffany&Co.',0,'/Storage/Plat/Brand/logo_360.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(361,'谢瑞麟',0,'/Storage/Plat/Brand/logo_361.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(362,'潮宏基',0,'/Storage/Plat/Brand/logo_362.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(363,'周大生',0,'/Storage/Plat/Brand/logo_363.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(364,'老凤祥',0,'/Storage/Plat/Brand/logo_364.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(365,'克徕帝',0,'/Storage/Plat/Brand/logo_365.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(366,'Acer',0,'/Storage/Plat/Brand/logo_366.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0'),(367,'佳能',0,'/Storage/Plat/Brand/logo_367.jpg',NULL,NULL,NULL,NULL,NULL,0,'\0');
/*!40000 ALTER TABLE `mall_brand` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_browsinghistory`
--

DROP TABLE IF EXISTS `mall_browsinghistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_browsinghistory` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `MemberId` BIGINT(20) NOT NULL COMMENT '会员ID',
  `ProductId` BIGINT(20) NOT NULL,
  `BrowseTime` DATETIME NOT NULL COMMENT '浏览时间',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_mall_BrowsingHistory_mall_BrowsingHistory` (`MemberId`) USING BTREE,
  KEY `FK_mall_BrowsingHistory_mall_Products` (`ProductId`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=4494 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_browsinghistory`
--

LOCK TABLES `mall_browsinghistory` WRITE;
/*!40000 ALTER TABLE `mall_browsinghistory` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_browsinghistory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_businesscategory`
--

DROP TABLE IF EXISTS `mall_businesscategory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_businesscategory` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `ShopId` BIGINT(20) NOT NULL,
  `CategoryId` BIGINT(20) NOT NULL COMMENT '分类ID',
  `CommisRate` DECIMAL(8,2) NOT NULL COMMENT '分佣比例',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Category_BusinessCategory` (`CategoryId`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=3689 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_businesscategory`
--

LOCK TABLES `mall_businesscategory` WRITE;
/*!40000 ALTER TABLE `mall_businesscategory` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_businesscategory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_businesscategoryapply`
--

DROP TABLE IF EXISTS `mall_businesscategoryapply`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_businesscategoryapply` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT COMMENT '主键',
  `ApplyDate` DATETIME NOT NULL COMMENT '申请日期',
  `ShopId` BIGINT(20) NOT NULL COMMENT '店铺ID',
  `ShopName` VARCHAR(100) NOT NULL COMMENT '店铺名称',
  `AuditedStatus` INT(11) NOT NULL COMMENT '审核状态',
  `AuditedDate` DATETIME DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_businesscategoryapply`
--

LOCK TABLES `mall_businesscategoryapply` WRITE;
/*!40000 ALTER TABLE `mall_businesscategoryapply` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_businesscategoryapply` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_businesscategoryapplydetail`
--

DROP TABLE IF EXISTS `mall_businesscategoryapplydetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_businesscategoryapplydetail` (
  `Id` BIGINT(11) NOT NULL AUTO_INCREMENT,
  `CommisRate` DECIMAL(8,2) NOT NULL COMMENT '分佣比例',
  `CategoryId` BIGINT(20) NOT NULL COMMENT '类目ID',
  `ApplyId` BIGINT(20) NOT NULL COMMENT '申请Id',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FR_BussinessCateApply` (`ApplyId`) USING BTREE,
  KEY `FR_BussinessCateApply_Cid` (`CategoryId`) USING BTREE
) ENGINE=INNODB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_businesscategoryapplydetail`
--

LOCK TABLES `mall_businesscategoryapplydetail` WRITE;
/*!40000 ALTER TABLE `mall_businesscategoryapplydetail` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_businesscategoryapplydetail` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_capital`
--

DROP TABLE IF EXISTS `mall_capital`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_capital` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `MemId` BIGINT(20) NOT NULL COMMENT '会员ID',
  `Balance` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '可用余额',
  `FreezeAmount` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '冻结资金',
  `ChargeAmount` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '累计充值总金额',
  `PresentAmount` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '累积充值赠送',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=74 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_capital`
--

LOCK TABLES `mall_capital` WRITE;
/*!40000 ALTER TABLE `mall_capital` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_capital` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_capitaldetail`
--

DROP TABLE IF EXISTS `mall_capitaldetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_capitaldetail` (
  `Id` BIGINT(20) NOT NULL,
  `CapitalID` BIGINT(20) NOT NULL COMMENT '资产主表ID',
  `SourceType` INT(11) NOT NULL COMMENT '资产类型',
  `Amount` DECIMAL(18,2) NOT NULL COMMENT '金额',
  `SourceData` VARCHAR(100) DEFAULT NULL COMMENT '来源数据',
  `CreateTime` DATETIME NOT NULL COMMENT '交易时间',
  `Remark` VARCHAR(255) DEFAULT NULL COMMENT '备注',
  `PresentAmount` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '赠送',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Reference_mall_CapitalDetail` (`CapitalID`) USING BTREE
) ENGINE=INNODB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_capitaldetail`
--

LOCK TABLES `mall_capitaldetail` WRITE;
/*!40000 ALTER TABLE `mall_capitaldetail` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_capitaldetail` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_cashdeposit`
--

DROP TABLE IF EXISTS `mall_cashdeposit`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_cashdeposit` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT COMMENT '主键ID',
  `ShopId` BIGINT(20) NOT NULL COMMENT 'Shop表外键',
  `CurrentBalance` DECIMAL(10,2) NOT NULL DEFAULT '0.00' COMMENT '可用金额',
  `TotalBalance` DECIMAL(10,2) NOT NULL DEFAULT '0.00' COMMENT '已缴纳金额',
  `Date` DATETIME NOT NULL COMMENT '最后一次缴纳时间',
  `EnableLabels` TINYINT(1) NOT NULL DEFAULT '1' COMMENT '是否显示标志，只有保证金欠费该字段才有用，默认显示',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_mall_CashDeposit_mall_Shops` (`ShopId`) USING BTREE
) ENGINE=INNODB DEFAULT CHARSET=gbk;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_cashdeposit`
--

LOCK TABLES `mall_cashdeposit` WRITE;
/*!40000 ALTER TABLE `mall_cashdeposit` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_cashdeposit` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_cashdepositdetail`
--

DROP TABLE IF EXISTS `mall_cashdepositdetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_cashdepositdetail` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `CashDepositId` BIGINT(20) NOT NULL DEFAULT '0',
  `AddDate` DATETIME NOT NULL,
  `Balance` DECIMAL(10,2) NOT NULL DEFAULT '0.00',
  `Operator` VARCHAR(50) NOT NULL COMMENT '操作类型',
  `Description` VARCHAR(1000) DEFAULT NULL COMMENT '说明',
  `RechargeWay` INT(11) DEFAULT NULL COMMENT '充值类型（银联、支付宝之类的）',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `KF_mall_CashDeposit_mall_CashDepositDetail` (`CashDepositId`) USING BTREE
) ENGINE=INNODB DEFAULT CHARSET=gbk;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_cashdepositdetail`
--

LOCK TABLES `mall_cashdepositdetail` WRITE;
/*!40000 ALTER TABLE `mall_cashdepositdetail` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_cashdepositdetail` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_category`
--

DROP TABLE IF EXISTS `mall_category`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_category` (
  `Id` BIGINT(20) NOT NULL,
  `Name` VARCHAR(100) NOT NULL COMMENT '分类名称',
  `Icon` VARCHAR(1000) DEFAULT NULL COMMENT '分类图标',
  `DisplaySequence` BIGINT(20) NOT NULL,
  `SupportVirtualProduct` BIT(1) NOT NULL DEFAULT b'0' COMMENT '是否支持虚拟商品(0=否，1=是)',
  `ParentCategoryId` BIGINT(20) NOT NULL,
  `Depth` INT(11) NOT NULL COMMENT '分类的深度',
  `Path` VARCHAR(100) NOT NULL COMMENT '分类的路径（以|分离）',
  `RewriteName` VARCHAR(50) DEFAULT NULL COMMENT '未使用',
  `HasChildren` TINYINT(1) NOT NULL COMMENT '是否有子分类',
  `TypeId` BIGINT(20) NOT NULL DEFAULT '0',
  `CommisRate` DECIMAL(8,2) NOT NULL COMMENT '分佣比例',
  `Meta_Title` VARCHAR(1000) DEFAULT NULL,
  `Meta_Description` VARCHAR(1000) DEFAULT NULL,
  `Meta_Keywords` VARCHAR(1000) DEFAULT NULL,
  `IsDeleted` BIT(1) NOT NULL COMMENT '是否已删除',
  `IsShow` TINYINT(1) NOT NULL DEFAULT '1' COMMENT '是否显示',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Type_Category` (`TypeId`) USING BTREE
) ENGINE=INNODB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_category`
--

LOCK TABLES `mall_category` WRITE;
/*!40000 ALTER TABLE `mall_category` DISABLE KEYS */;
INSERT INTO `mall_category` VALUES (1,'食品、酒类、特产','/Storage/Plat/ImageAd/201702141458045546690.png',1,'\0',0,1,'1',NULL,0,82,100.00,NULL,NULL,NULL,'\0',1),(2,'坚果',NULL,1,'\0',1,2,'1|2',NULL,0,82,100.00,NULL,NULL,NULL,'\0',1),(3,'松子',NULL,1,'\0',2,3,'1|2|3',NULL,0,82,2.00,NULL,NULL,NULL,'\0',1),(4,'辣条',NULL,2,'\0',1,2,'1|4',NULL,0,82,100.00,NULL,NULL,NULL,'\0',1),(5,'卫龙',NULL,1,'\0',4,3,'1|4|5',NULL,0,82,1.00,NULL,NULL,NULL,'\0',1),(6,'核桃',NULL,2,'\0',2,3,'1|2|6',NULL,0,82,1.00,NULL,NULL,NULL,'\0',1),(7,'碧根果',NULL,3,'\0',2,3,'1|2|7',NULL,0,82,2.00,NULL,NULL,NULL,'\0',1),(8,'男装、女装、童装','/Storage/Plat/ImageAd/201702141457468202460.png',2,'\0',0,1,'8',NULL,0,83,100.00,NULL,NULL,NULL,'\0',1),(9,'女装',NULL,1,'\0',8,2,'8|9',NULL,0,83,100.00,NULL,NULL,NULL,'\0',1),(10,'外套',NULL,1,'\0',9,3,'8|9|10',NULL,0,83,2.00,NULL,NULL,NULL,'\0',1),(11,'裙子',NULL,2,'\0',9,3,'8|9|11',NULL,0,83,2.00,NULL,NULL,NULL,'\0',1),(12,'衬衣',NULL,3,'\0',9,3,'8|9|12',NULL,0,83,3.00,NULL,NULL,NULL,'\0',1),(13,'针织衫',NULL,4,'\0',9,3,'8|9|13',NULL,0,83,2.00,NULL,NULL,NULL,'\0',1),(14,'电脑办公','/Storage/Plat/ImageAd/201702141501542881100.png',3,'\0',0,1,'14',NULL,0,84,100.00,NULL,NULL,NULL,'\0',1),(15,'电脑整机',NULL,1,'\0',14,2,'14|15',NULL,0,84,100.00,NULL,NULL,NULL,'\0',1),(16,'笔记本',NULL,1,'\0',15,3,'14|15|16',NULL,0,84,1.00,NULL,NULL,NULL,'\0',1),(17,'生鲜','/Storage/Plat/ImageAd/201702141501257655790.png',4,'\0',0,1,'17',NULL,0,85,100.00,NULL,NULL,NULL,'\0',1),(18,'海鲜水产',NULL,1,'\0',17,2,'17|18',NULL,0,85,100.00,NULL,NULL,NULL,'\0',1),(19,'虾类',NULL,1,'\0',18,3,'17|18|19',NULL,0,85,2.00,NULL,NULL,NULL,'\0',1),(20,'个护化妆、清洁用品','/Storage/Plat/ImageAd/201702141458340605160.png',5,'\0',0,1,'20',NULL,0,87,100.00,NULL,NULL,NULL,'\0',1),(21,'面部护肤',NULL,1,'\0',20,2,'20|21',NULL,0,87,100.00,NULL,NULL,NULL,'\0',1),(22,'洁面',NULL,1,'\0',21,3,'20|21|22',NULL,0,87,3.00,NULL,NULL,NULL,'\0',1),(23,'卸妆',NULL,2,'\0',21,3,'20|21|23',NULL,0,87,3.00,NULL,NULL,NULL,'\0',1),(24,'家用电器','/Storage/Plat/ImageAd/201702141501082050620.png',6,'\0',0,1,'24',NULL,0,86,100.00,NULL,NULL,NULL,'\0',1),(25,'家居、家具、家装','/Storage/Plat/ImageAd/201702141501364804300.png',7,'\0',0,1,'25',NULL,0,88,100.00,NULL,NULL,NULL,'\0',1),(26,'厨具',NULL,1,'\0',25,2,'25|26',NULL,0,88,100.00,NULL,NULL,NULL,'\0',1),(27,'烹饪锅具',NULL,2,'\0',25,2,'25|27',NULL,0,88,100.00,NULL,NULL,NULL,'',1),(28,'烹饪锅具',NULL,1,'\0',26,3,'25|26|28',NULL,0,88,2.00,NULL,NULL,NULL,'\0',1),(29,'厨房配件',NULL,2,'\0',26,3,'25|26|29',NULL,0,88,2.00,NULL,NULL,NULL,'\0',1),(30,'餐具',NULL,3,'\0',26,3,'25|26|30',NULL,0,88,2.00,NULL,NULL,NULL,'\0',1),(31,'家纺',NULL,2,'\0',25,2,'25|31',NULL,0,88,100.00,NULL,NULL,NULL,'\0',1),(32,'床品套件',NULL,1,'\0',31,3,'25|31|32',NULL,0,88,3.00,NULL,NULL,NULL,'\0',1),(33,'被子',NULL,2,'\0',31,3,'25|31|33',NULL,0,88,3.00,NULL,NULL,NULL,'\0',1),(34,'枕头',NULL,3,'\0',31,3,'25|31|34',NULL,0,88,2.00,NULL,NULL,NULL,'\0',1),(35,'家装软式',NULL,3,'\0',25,2,'25|35',NULL,0,88,100.00,NULL,NULL,NULL,'\0',1),(36,'装饰字画',NULL,1,'\0',35,3,'25|35|36',NULL,0,88,2.00,NULL,NULL,NULL,'\0',1),(37,'装饰摆件',NULL,2,'\0',35,3,'25|35|37',NULL,0,88,2.00,NULL,NULL,NULL,'\0',1),(38,'窗帘隔断',NULL,3,'\0',35,3,'25|35|38',NULL,0,88,2.00,NULL,NULL,NULL,'\0',1),(39,'鞋靴、箱包','/Storage/Plat/ImageAd/201702141459041288400.png',8,'\0',0,1,'39',NULL,0,89,100.00,NULL,NULL,NULL,'\0',1),(40,'运动、户外','/Storage/Plat/ImageAd/201702141459248710700.png',9,'\0',0,1,'40',NULL,0,90,100.00,NULL,NULL,NULL,'\0',1),(41,'母婴、玩具乐器','/Storage/Plat/ImageAd/201702141459334413860.png',10,'\0',0,1,'41',NULL,0,92,100.00,NULL,NULL,NULL,'\0',1),(42,'医药保健','/Storage/Plat/ImageAd/201702141459487451100.png',11,'\0',0,1,'42',NULL,0,93,100.00,NULL,NULL,NULL,'\0',1),(43,'图书、影像、电子书','/Storage/Plat/ImageAd/201702141500082450630.png',12,'\0',0,1,'43',NULL,0,94,100.00,NULL,NULL,NULL,'\0',1),(44,'手机、 数码','/Storage/Plat/ImageAd/201702141459577177240.png',13,'\0',0,1,'44',NULL,0,84,100.00,NULL,NULL,NULL,'\0',1),(45,'汽车、汽车用品','/Storage/Plat/ImageAd/201702141500254013550.png',14,'\0',0,1,'45',NULL,0,91,100.00,NULL,NULL,NULL,'\0',1),(46,'电视',NULL,1,'\0',24,2,'24|46',NULL,0,86,100.00,NULL,NULL,NULL,'\0',1),(47,'空调',NULL,2,'\0',24,2,'24|47',NULL,0,86,100.00,NULL,NULL,NULL,'\0',1),(48,'洗衣机',NULL,3,'\0',24,2,'24|48',NULL,0,86,100.00,NULL,NULL,NULL,'\0',1),(49,'厨卫大电',NULL,4,'\0',24,2,'24|49',NULL,0,86,100.00,NULL,NULL,NULL,'\0',1),(50,'生活电器',NULL,5,'\0',24,2,'24|50',NULL,0,86,100.00,NULL,NULL,NULL,'\0',1),(51,'合资品牌',NULL,1,'\0',46,3,'24|46|51',NULL,0,86,3.00,NULL,NULL,NULL,'\0',1),(52,'国产品牌',NULL,2,'\0',46,3,'24|46|52',NULL,0,86,2.00,NULL,NULL,NULL,'\0',1),(53,'互联网品牌',NULL,3,'\0',46,3,'24|46|53',NULL,0,86,3.00,NULL,NULL,NULL,'\0',1),(54,'壁挂式空调',NULL,1,'\0',47,3,'24|47|54',NULL,0,86,2.00,NULL,NULL,NULL,'\0',1),(55,'柜式空调',NULL,2,'\0',47,3,'24|47|55',NULL,0,86,2.00,NULL,NULL,NULL,'\0',1),(56,'中央空调',NULL,3,'\0',47,3,'24|47|56',NULL,0,86,4.00,NULL,NULL,NULL,'\0',1),(57,'空调配件',NULL,4,'\0',47,3,'24|47|57',NULL,0,86,1.00,NULL,NULL,NULL,'\0',1),(58,'滚筒洗衣机',NULL,1,'\0',48,3,'24|48|58',NULL,0,86,3.00,NULL,NULL,NULL,'\0',1),(59,'机烘一体机',NULL,2,'\0',48,3,'24|48|59',NULL,0,86,5.00,NULL,NULL,NULL,'\0',1),(60,'波轮洗衣机',NULL,3,'\0',48,3,'24|48|60',NULL,0,86,5.00,NULL,NULL,NULL,'\0',1),(61,'迷你洗衣机',NULL,4,'\0',48,3,'24|48|61',NULL,0,86,5.00,NULL,NULL,NULL,'\0',1),(62,'洗衣机配件',NULL,5,'\0',48,3,'24|48|62',NULL,0,86,2.00,NULL,NULL,NULL,'\0',1),(63,'油烟机',NULL,1,'\0',49,3,'24|49|63',NULL,0,86,2.00,NULL,NULL,NULL,'\0',1),(64,'燃气灶',NULL,2,'\0',49,3,'24|49|64',NULL,0,86,5.00,NULL,NULL,NULL,'\0',1),(65,'吸尘器',NULL,1,'\0',50,3,'24|50|65',NULL,0,86,7.00,NULL,NULL,NULL,'\0',1),(66,'进化器',NULL,2,'\0',50,3,'24|50|66',NULL,0,86,3.00,NULL,NULL,NULL,'\0',1),(67,'加湿器',NULL,3,'\0',50,3,'24|50|67',NULL,0,86,3.00,NULL,NULL,NULL,'\0',1),(68,'时尚女鞋',NULL,1,'\0',39,2,'39|68',NULL,0,89,100.00,NULL,NULL,NULL,'\0',1),(69,'流行男鞋',NULL,2,'\0',39,2,'39|69',NULL,0,89,100.00,NULL,NULL,NULL,'\0',1),(70,'潮流女包',NULL,3,'\0',39,2,'39|70',NULL,0,89,100.00,NULL,NULL,NULL,'\0',1),(71,'精品男包',NULL,4,'\0',39,2,'39|71',NULL,0,89,100.00,NULL,NULL,NULL,'\0',1),(72,'女靴',NULL,1,'\0',68,3,'39|68|72',NULL,0,89,4.00,NULL,NULL,NULL,'\0',1),(73,'单鞋',NULL,2,'\0',68,3,'39|68|73',NULL,0,89,4.00,NULL,NULL,NULL,'\0',1),(74,'布鞋',NULL,3,'\0',68,3,'39|68|74',NULL,0,89,2.00,NULL,NULL,NULL,'\0',1),(75,'休闲鞋',NULL,1,'\0',69,3,'39|69|75',NULL,0,89,2.00,NULL,NULL,NULL,'\0',1),(76,'正装鞋',NULL,2,'\0',69,3,'39|69|76',NULL,0,89,4.00,NULL,NULL,NULL,'\0',1),(77,'运动鞋',NULL,3,'\0',69,3,'39|69|77',NULL,0,89,3.00,NULL,NULL,NULL,'\0',1),(78,'男靴',NULL,4,'\0',69,3,'39|69|78',NULL,0,89,5.00,NULL,NULL,NULL,'\0',1),(79,'凉鞋',NULL,5,'\0',69,3,'39|69|79',NULL,0,89,5.00,NULL,NULL,NULL,'\0',1),(80,'凉鞋',NULL,4,'\0',68,3,'39|68|80',NULL,0,89,4.00,NULL,NULL,NULL,'\0',1),(81,'真皮包',NULL,1,'\0',70,3,'39|70|81',NULL,0,89,5.00,NULL,NULL,NULL,'\0',1),(82,'水桶包',NULL,2,'\0',70,3,'39|70|82',NULL,0,89,4.00,NULL,NULL,NULL,'\0',1),(83,'斜挎包',NULL,3,'\0',70,3,'39|70|83',NULL,0,89,4.00,NULL,NULL,NULL,'\0',1),(84,'手提包',NULL,4,'\0',70,3,'39|70|84',NULL,0,89,3.00,NULL,NULL,NULL,'\0',1),(85,'帆布包',NULL,5,'\0',70,3,'39|70|85',NULL,0,89,3.00,NULL,NULL,NULL,'\0',1),(86,'商务包',NULL,1,'\0',71,3,'39|71|86',NULL,0,89,5.00,NULL,NULL,NULL,'\0',1),(87,'双肩背包',NULL,2,'\0',71,3,'39|71|87',NULL,0,89,4.00,NULL,NULL,NULL,'\0',1),(88,'男士手包',NULL,3,'\0',71,3,'39|71|88',NULL,0,89,3.00,NULL,NULL,NULL,'\0',1),(89,'运动鞋包',NULL,1,'\0',40,2,'40|89',NULL,0,90,100.00,NULL,NULL,NULL,'\0',1),(90,'运动服饰',NULL,2,'\0',40,2,'40|90',NULL,0,90,100.00,NULL,NULL,NULL,'\0',1),(91,'骑行运动',NULL,3,'\0',40,2,'40|91',NULL,0,90,100.00,NULL,NULL,NULL,'\0',1),(92,'体育用品',NULL,4,'\0',40,2,'40|92',NULL,0,90,100.00,NULL,NULL,NULL,'\0',1),(93,'跑步鞋',NULL,1,'\0',89,3,'40|89|93',NULL,0,90,5.00,NULL,NULL,NULL,'\0',1),(94,'篮球鞋',NULL,2,'\0',89,3,'40|89|94',NULL,0,90,3.00,NULL,NULL,NULL,'\0',1),(95,'帆布鞋',NULL,3,'\0',89,3,'40|89|95',NULL,0,90,4.00,NULL,NULL,NULL,'\0',1),(96,'登山鞋',NULL,4,'\0',89,3,'40|89|96',NULL,0,90,4.00,NULL,NULL,NULL,'\0',1),(97,'足球鞋',NULL,5,'\0',89,3,'40|89|97',NULL,0,90,4.00,NULL,NULL,NULL,'\0',1),(98,'T恤',NULL,1,'\0',90,3,'40|90|98',NULL,0,90,4.00,NULL,NULL,NULL,'\0',1),(99,'运动裤',NULL,2,'\0',90,3,'40|90|99',NULL,0,90,3.00,NULL,NULL,NULL,'\0',1),(100,'汗巾',NULL,3,'\0',90,3,'40|90|100',NULL,0,90,4.00,NULL,NULL,NULL,'\0',1),(101,'护腕',NULL,4,'\0',90,3,'40|90|101',NULL,0,90,4.00,NULL,NULL,NULL,'\0',1),(102,'山地车',NULL,1,'\0',91,3,'40|91|102',NULL,0,90,5.00,NULL,NULL,NULL,'\0',1),(103,'自行车',NULL,2,'\0',91,3,'40|91|103',NULL,0,90,6.00,NULL,NULL,NULL,'\0',1),(104,'折叠车',NULL,3,'\0',91,3,'40|91|104',NULL,0,90,5.00,NULL,NULL,NULL,'\0',1),(105,'骑行装备',NULL,4,'\0',91,3,'40|91|105',NULL,0,90,4.00,NULL,NULL,NULL,'\0',1),(106,'羽毛球',NULL,1,'\0',92,3,'40|92|106',NULL,0,90,2.00,NULL,NULL,NULL,'\0',1),(107,'乒乓球',NULL,2,'\0',92,3,'40|92|107',NULL,0,90,3.00,NULL,NULL,NULL,'\0',1),(108,'篮球',NULL,3,'\0',92,3,'40|92|108',NULL,0,90,5.00,NULL,NULL,NULL,'\0',1),(109,'足球',NULL,4,'\0',92,3,'40|92|109',NULL,0,90,4.00,NULL,NULL,NULL,'\0',1),(110,'棒球',NULL,5,'\0',92,3,'40|92|110',NULL,0,90,4.00,NULL,NULL,NULL,'\0',1),(111,'网球',NULL,6,'\0',92,3,'40|92|111',NULL,0,90,3.00,NULL,NULL,NULL,'\0',1),(112,'高尔夫',NULL,7,'\0',92,3,'40|92|112',NULL,0,90,5.00,NULL,NULL,NULL,'\0',1),(113,'台球',NULL,8,'\0',92,3,'40|92|113',NULL,0,90,4.00,NULL,NULL,NULL,'\0',1),(114,'奶粉',NULL,1,'\0',41,2,'41|114',NULL,0,92,100.00,NULL,NULL,NULL,'\0',1),(115,'营养辅食',NULL,2,'\0',41,2,'41|115',NULL,0,92,100.00,NULL,NULL,NULL,'\0',1),(116,'尿裤湿巾',NULL,3,'\0',41,2,'41|116',NULL,0,92,100.00,NULL,NULL,NULL,'\0',1),(117,'1段',NULL,1,'\0',114,3,'41|114|117',NULL,0,92,3.00,NULL,NULL,NULL,'\0',1),(118,'2段',NULL,2,'\0',114,3,'41|114|118',NULL,0,92,3.00,NULL,NULL,NULL,'\0',1),(119,'3段',NULL,3,'\0',114,3,'41|114|119',NULL,0,92,4.00,NULL,NULL,NULL,'\0',1),(120,'4段',NULL,4,'\0',114,3,'41|114|120',NULL,0,92,3.00,NULL,NULL,NULL,'\0',1),(121,'孕妈奶粉',NULL,5,'\0',114,3,'41|114|121',NULL,0,92,3.00,NULL,NULL,NULL,'\0',1),(122,'米粉、菜粉',NULL,1,'\0',115,3,'41|115|122',NULL,0,92,3.00,NULL,NULL,NULL,'\0',1),(123,'DHA',NULL,2,'\0',115,3,'41|115|123',NULL,0,92,4.00,NULL,NULL,NULL,'\0',1),(124,'拉拉裤',NULL,1,'\0',116,3,'41|116|124',NULL,0,92,4.00,NULL,NULL,NULL,'\0',1),(125,'婴儿湿巾',NULL,2,'\0',116,3,'41|116|125',NULL,0,92,4.00,NULL,NULL,NULL,'\0',1),(126,'中西药品',NULL,1,'\0',42,2,'42|126',NULL,0,93,100.00,NULL,NULL,NULL,'\0',1),(127,'营养健康',NULL,2,'\0',42,2,'42|127',NULL,0,93,100.00,NULL,NULL,NULL,'\0',1),(128,'感冒咳嗽',NULL,1,'\0',126,3,'42|126|128',NULL,0,93,4.00,NULL,NULL,NULL,'\0',1),(129,'补气养血',NULL,2,'\0',126,3,'42|126|129',NULL,0,93,3.00,NULL,NULL,NULL,'\0',1),(130,'调节免疫',NULL,1,'\0',127,3,'42|127|130',NULL,0,93,4.00,NULL,NULL,NULL,'\0',1),(131,'调节三高',NULL,2,'\0',127,3,'42|127|131',NULL,0,93,3.00,NULL,NULL,NULL,'\0',1),(132,'手机通讯',NULL,1,'\0',44,2,'44|132',NULL,0,84,100.00,NULL,NULL,NULL,'\0',1),(133,'手机配件',NULL,2,'\0',44,2,'44|133',NULL,0,84,100.00,NULL,NULL,NULL,'\0',1),(134,'手机',NULL,1,'\0',132,3,'44|132|134',NULL,0,84,5.00,NULL,NULL,NULL,'\0',1),(135,'对讲机',NULL,2,'\0',132,3,'44|132|135',NULL,0,84,4.00,NULL,NULL,NULL,'\0',1),(136,'手机壳',NULL,1,'\0',133,3,'44|133|136',NULL,0,84,5.00,NULL,NULL,NULL,'\0',1),(137,'贴膜',NULL,2,'\0',133,3,'44|133|137',NULL,0,84,4.00,NULL,NULL,NULL,'\0',1),(138,'数据线',NULL,3,'\0',133,3,'44|133|138',NULL,0,84,3.00,NULL,NULL,NULL,'\0',1),(139,'充电器',NULL,4,'\0',133,3,'44|133|139',NULL,0,84,3.00,NULL,NULL,NULL,'\0',1),(140,'耳机',NULL,5,'\0',133,3,'44|133|140',NULL,0,84,3.00,NULL,NULL,NULL,'\0',1),(141,'汽车装饰',NULL,1,'\0',45,2,'45|141',NULL,0,91,100.00,NULL,NULL,NULL,'\0',1),(142,'美容清洗',NULL,2,'\0',45,2,'45|142',NULL,0,91,100.00,NULL,NULL,NULL,'\0',1),(143,'脚垫',NULL,1,'\0',141,3,'45|141|143',NULL,0,91,4.00,NULL,NULL,NULL,'\0',1),(144,'座套',NULL,2,'\0',141,3,'45|141|144',NULL,0,91,3.00,NULL,NULL,NULL,'\0',1),(145,'方向盘套',NULL,3,'\0',141,3,'45|141|145',NULL,0,91,4.00,NULL,NULL,NULL,'\0',1),(146,'后备箱垫',NULL,4,'\0',141,3,'45|141|146',NULL,0,91,3.00,NULL,NULL,NULL,'\0',1),(147,'车蜡',NULL,1,'\0',142,3,'45|142|147',NULL,0,91,3.00,NULL,NULL,NULL,'\0',1),(148,'补漆笔',NULL,2,'\0',142,3,'45|142|148',NULL,0,91,4.00,NULL,NULL,NULL,'\0',1),(149,'玻璃水',NULL,3,'\0',142,3,'45|142|149',NULL,0,91,3.00,NULL,NULL,NULL,'\0',1),(150,'清洁剂',NULL,4,'\0',142,3,'45|142|150',NULL,0,91,3.00,NULL,NULL,NULL,'\0',1),(151,'钟表','/Storage/Plat/ImageAd/201702141500376386730.png',15,'\0',0,1,'151',NULL,0,95,100.00,NULL,NULL,NULL,'\0',1),(152,'珠宝','/Storage/Plat/ImageAd/201702141500439120730.png',16,'\0',0,1,'152',NULL,0,96,100.00,NULL,NULL,NULL,'\0',1),(153,'水果',NULL,3,'\0',1,2,'1|153',NULL,0,82,100.00,NULL,NULL,NULL,'\0',1),(154,'水果',NULL,1,'\0',153,3,'1|153|154',NULL,0,82,1.00,NULL,NULL,NULL,'\0',1),(155,'数码产品',NULL,3,'\0',44,2,'44|155',NULL,0,84,100.00,NULL,NULL,NULL,'\0',1),(156,'视频类用品',NULL,1,'\0',155,3,'44|155|156',NULL,0,84,5.00,NULL,NULL,NULL,'\0',1),(157,'面部护理',NULL,3,'\0',21,3,'20|21|157',NULL,0,87,5.00,NULL,NULL,NULL,'\0',1),(158,'手表',NULL,1,'\0',151,2,'151|158',NULL,0,95,100.00,NULL,NULL,NULL,'\0',1),(159,'男表',NULL,1,'\0',158,3,'151|158|159',NULL,0,95,8.00,NULL,NULL,NULL,'\0',1);
/*!40000 ALTER TABLE `mall_category` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_categorycashdeposit`
--

DROP TABLE IF EXISTS `mall_categorycashdeposit`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_categorycashdeposit` (
  `Id` BIGINT(20) NOT NULL COMMENT '主键Id',
  `CategoryId` BIGINT(20) NOT NULL COMMENT '分类Id',
  `NeedPayCashDeposit` DECIMAL(10,2) NOT NULL DEFAULT '0.00' COMMENT '需要缴纳保证金',
  `EnableNoReasonReturn` TINYINT(1) NOT NULL DEFAULT '1' COMMENT '允许七天无理由退货',
  PRIMARY KEY (`CategoryId`) USING BTREE,
  KEY `FK_mall_CategoriesObligation_Categories` (`CategoryId`) USING BTREE
) ENGINE=INNODB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_categorycashdeposit`
--

LOCK TABLES `mall_categorycashdeposit` WRITE;
/*!40000 ALTER TABLE `mall_categorycashdeposit` DISABLE KEYS */;
INSERT INTO `mall_categorycashdeposit` VALUES (0,1,0.00,0),(0,8,0.00,0),(0,14,0.00,0),(0,17,0.00,0),(0,20,0.00,0),(0,24,0.00,0),(0,25,0.00,0),(0,39,0.00,0),(0,40,0.00,0),(0,41,0.00,0),(0,42,0.00,0),(0,43,0.00,0),(0,44,0.00,0),(0,45,0.00,0),(0,151,0.00,0),(0,152,0.00,0);
/*!40000 ALTER TABLE `mall_categorycashdeposit` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_chargedetail`
--

DROP TABLE IF EXISTS `mall_chargedetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_chargedetail` (
  `Id` BIGINT(20) NOT NULL,
  `MemId` BIGINT(20) NOT NULL COMMENT '会员ID',
  `ChargeTime` DATETIME DEFAULT NULL COMMENT '充值时间',
  `ChargeAmount` DECIMAL(18,2) NOT NULL COMMENT '充值金额',
  `ChargeWay` VARCHAR(50) DEFAULT NULL COMMENT '充值方式',
  `ChargeStatus` INT(11) NOT NULL COMMENT '充值状态',
  `CreateTime` DATETIME NOT NULL COMMENT '提交充值时间',
  `PresentAmount` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '赠送',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_chargedetail`
--

LOCK TABLES `mall_chargedetail` WRITE;
/*!40000 ALTER TABLE `mall_chargedetail` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_chargedetail` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_chargedetailshop`
--

DROP TABLE IF EXISTS `mall_chargedetailshop`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_chargedetailshop` (
  `Id` BIGINT(20) NOT NULL,
  `ShopId` BIGINT(20) NOT NULL COMMENT '店铺ID',
  `ChargeTime` DATETIME DEFAULT NULL COMMENT '充值时间',
  `ChargeAmount` DECIMAL(18,2) NOT NULL COMMENT '充值金额',
  `ChargeWay` VARCHAR(50) DEFAULT NULL COMMENT '充值方式',
  `ChargeStatus` INT(11) NOT NULL COMMENT '充值状态',
  `CreateTime` DATETIME NOT NULL COMMENT '提交充值时间',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_chargedetailshop`
--

LOCK TABLES `mall_chargedetailshop` WRITE;
/*!40000 ALTER TABLE `mall_chargedetailshop` DISABLE KEYS */;
INSERT INTO `mall_chargedetailshop` VALUES (17021411310728770,1,NULL,5000.00,NULL,1,'2017-02-14 11:31:07'),(17021411323326291,1,NULL,5000.00,NULL,1,'2017-02-14 11:32:34');
/*!40000 ALTER TABLE `mall_chargedetailshop` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_cityexpressconfig`
--

DROP TABLE IF EXISTS `mall_cityexpressconfig`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_cityexpressconfig` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `ShopId` BIGINT(20) NOT NULL COMMENT '商家编号',
  `IsEnable` TINYINT(1) NOT NULL DEFAULT '0' COMMENT '是否开启',
  `source_id` VARCHAR(200) DEFAULT NULL COMMENT '商户号',
  `app_key` VARCHAR(200) DEFAULT NULL COMMENT 'appKey',
  `app_secret` VARCHAR(200) DEFAULT NULL COMMENT 'appSecret',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_cityexpressconfig`
--

LOCK TABLES `mall_cityexpressconfig` WRITE;
/*!40000 ALTER TABLE `mall_cityexpressconfig` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_cityexpressconfig` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_collocation`
--

DROP TABLE IF EXISTS `mall_collocation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_collocation` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT COMMENT 'ID自增',
  `Title` VARCHAR(100) NOT NULL COMMENT '组合购标题',
  `StartTime` DATETIME NOT NULL COMMENT '开始日期',
  `EndTime` DATETIME NOT NULL COMMENT '结束日期',
  `ShortDesc` VARCHAR(1000) DEFAULT NULL COMMENT '组合描述',
  `ShopId` BIGINT(20) NOT NULL COMMENT '组合购店铺ID',
  `CreateTime` DATETIME DEFAULT NULL COMMENT '添加时间',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=98 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_collocation`
--

LOCK TABLES `mall_collocation` WRITE;
/*!40000 ALTER TABLE `mall_collocation` DISABLE KEYS */;
INSERT INTO `mall_collocation` VALUES (21,'组合购','2017-02-14 00:00:00','2017-05-06 00:00:00','',1,'2017-02-14 11:56:42'),(22,'组合购','2017-02-14 00:00:00','2017-04-28 00:00:00','',1,'2017-02-14 11:57:17'),(23,'服装组合购','2017-02-14 00:00:00','2017-09-22 00:00:00','',1,'2017-02-14 11:57:54'),(24,'三只松鼠优惠','2017-02-14 00:00:00','2017-09-16 00:00:00','',1,'2017-02-14 11:58:36'),(25,'电脑大酬宾','2017-02-14 00:00:00','2017-11-17 00:00:00','',1,'2017-02-14 11:59:15');
/*!40000 ALTER TABLE `mall_collocation` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_collocationporuduct`
--

DROP TABLE IF EXISTS `mall_collocationporuduct`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_collocationporuduct` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT COMMENT 'ID自增',
  `ProductId` BIGINT(20) NOT NULL COMMENT '商品ID',
  `ColloId` BIGINT(20) NOT NULL COMMENT '组合购ID',
  `IsMain` TINYINT(1) NOT NULL COMMENT '是否主商品',
  `DisplaySequence` INT(11) NOT NULL COMMENT '排序',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Collocation_CollPoruducts` (`ColloId`) USING BTREE,
  KEY `FK_Product_CollPoruducts` (`ProductId`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=368 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_collocationporuduct`
--

LOCK TABLES `mall_collocationporuduct` WRITE;
/*!40000 ALTER TABLE `mall_collocationporuduct` DISABLE KEYS */;
INSERT INTO `mall_collocationporuduct` VALUES (132,717,21,1,0),(133,718,21,0,1),(134,713,22,1,0),(135,712,22,0,1),(136,708,23,1,0),(137,707,23,0,1),(138,701,24,1,0),(139,700,24,0,1),(140,715,25,1,0),(141,714,25,0,1);
/*!40000 ALTER TABLE `mall_collocationporuduct` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_collocationsku`
--

DROP TABLE IF EXISTS `mall_collocationsku`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_collocationsku` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT COMMENT 'ID自增',
  `ProductId` BIGINT(20) NOT NULL COMMENT '商品ID',
  `SkuID` VARCHAR(100) NOT NULL COMMENT '商品SkuId',
  `ColloProductId` BIGINT(20) NOT NULL COMMENT '组合商品表ID',
  `Price` DECIMAL(18,2) NOT NULL COMMENT '组合购价格',
  `SkuPirce` DECIMAL(18,2) NOT NULL COMMENT '原始价格',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Product_CollSkus` (`ProductId`) USING BTREE,
  KEY `FK_ColloPoruducts_CollSkus` (`ColloProductId`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=902 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_collocationsku`
--

LOCK TABLES `mall_collocationsku` WRITE;
/*!40000 ALTER TABLE `mall_collocationsku` DISABLE KEYS */;
INSERT INTO `mall_collocationsku` VALUES (400,717,'717_0_0_0',132,18.00,19.90),(401,718,'718_0_0_0',133,100.00,110.00),(402,713,'713_0_0_0',134,45.00,50.00),(403,712,'712_0_661_0',135,213.00,229.00),(404,712,'712_0_659_0',135,213.00,229.00),(405,708,'708_0_0_0',136,400.00,450.00),(406,707,'707_655_663_0',137,160.00,175.00),(407,701,'701_0_0_0',138,25.00,29.90),(408,700,'700_0_0_0',139,55.00,59.90),(409,715,'715_0_0_0',140,12000.00,13688.00),(410,714,'714_0_0_0',141,6500.00,6988.00);
/*!40000 ALTER TABLE `mall_collocationsku` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_coupon`
--

DROP TABLE IF EXISTS `mall_coupon`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_coupon` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `ShopId` BIGINT(20) NOT NULL,
  `ShopName` VARCHAR(100) DEFAULT NULL COMMENT '店铺名称',
  `Price` DECIMAL(18,0) NOT NULL COMMENT '价格',
  `PerMax` INT(11) NOT NULL COMMENT '最大可领取张数',
  `OrderAmount` DECIMAL(18,0) NOT NULL COMMENT '订单金额（满足多少钱才能使用）',
  `Num` INT(11) NOT NULL COMMENT '发行张数',
  `StartTime` DATETIME NOT NULL COMMENT '开始时间',
  `EndTime` DATETIME NOT NULL,
  `CouponName` VARCHAR(100) NOT NULL COMMENT '优惠券名称',
  `CreateTime` DATETIME NOT NULL,
  `ReceiveType` INT(11) NOT NULL DEFAULT '0' COMMENT '领取方式 0 店铺首页 1 积分兑换 2 主动发放',
  `NeedIntegral` INT(11) NOT NULL COMMENT '所需积分',
  `EndIntegralExchange` DATETIME NOT NULL COMMENT '兑换截止时间',
  `IntegralCover` VARCHAR(200) DEFAULT NULL COMMENT '积分商城封面',
  `IsSyncWeiXin` INT(11) NOT NULL DEFAULT '0' COMMENT '是否同步到微信',
  `WXAuditStatus` INT(11) NOT NULL DEFAULT '0' COMMENT '微信状态',
  `CardLogId` BIGINT(20) DEFAULT NULL COMMENT '微信卡券记录号 与微信卡券记录关联',
  `UseArea` INT(1) NOT NULL DEFAULT '0' COMMENT '使用范围：0=全场通用，1=部分商品可用',
  `Remark` VARCHAR(500) DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_mall_Coupon_mall_Shops` (`ShopId`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=194 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_coupon`
--

LOCK TABLES `mall_coupon` WRITE;
/*!40000 ALTER TABLE `mall_coupon` DISABLE KEYS */;
INSERT INTO `mall_coupon` VALUES (59,1,'非凡资源店',20,1,100,100,'2017-02-15 00:00:00','2017-02-13 00:00:00','20元优惠券','2017-02-14 11:51:24',1,10,'2017-06-16 00:00:00',NULL,0,1,NULL,0,NULL),(60,1,'官方自营店',50,1,1300,50,'2017-03-16 00:00:00','2017-05-05 00:00:00','50元优惠券','2017-02-14 11:53:41',0,0,'2018-06-16 00:00:00',NULL,0,1,NULL,0,NULL),(61,1,'官方自营店',100,1,499,20,'2017-02-14 00:00:00','2017-03-14 00:00:00','100元优惠券','2017-02-14 11:54:02',0,0,'2018-06-16 00:00:00',NULL,0,1,NULL,0,NULL),(62,1,'官方自营店',10,0,100,1000,'2017-02-26 00:00:00','2017-07-14 00:00:00','10元优惠券','2017-02-14 11:54:45',0,0,'2018-06-16 00:00:00',NULL,0,1,NULL,0,NULL),(63,1,'官方自营店',5,0,10,100,'2017-02-24 00:00:00','2017-06-01 00:00:00','5元优惠券','2017-02-14 11:55:12',2,0,'2018-06-16 00:00:00',NULL,0,1,NULL,0,NULL),(64,1,'官方自营店',20,0,50,10000,'2017-02-14 00:00:00','2017-09-16 00:00:00','20元优惠券','2017-02-14 17:50:28',1,10,'2017-09-16 17:45:46',NULL,0,1,NULL,0,NULL);
/*!40000 ALTER TABLE `mall_coupon` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_couponproduct`
--

DROP TABLE IF EXISTS `mall_couponproduct`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_couponproduct` (
  `Id` INT(11) NOT NULL AUTO_INCREMENT,
  `CouponId` BIGINT(20) NOT NULL,
  `ProductId` BIGINT(20) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `CouponId` (`CouponId`) USING BTREE,
  KEY `ProductId` (`ProductId`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=27 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_couponproduct`
--

LOCK TABLES `mall_couponproduct` WRITE;
/*!40000 ALTER TABLE `mall_couponproduct` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_couponproduct` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_couponrecord`
--

DROP TABLE IF EXISTS `mall_couponrecord`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_couponrecord` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `CouponId` BIGINT(20) NOT NULL,
  `CounponSN` VARCHAR(50) NOT NULL COMMENT '优惠券的SN标示',
  `CounponTime` DATETIME NOT NULL,
  `UserName` VARCHAR(100) NOT NULL COMMENT '用户名称',
  `UserId` BIGINT(20) NOT NULL,
  `UsedTime` DATETIME DEFAULT NULL,
  `OrderId` BIGINT(20) DEFAULT NULL COMMENT '使用的订单ID',
  `ShopId` BIGINT(20) NOT NULL,
  `ShopName` VARCHAR(100) NOT NULL,
  `CounponStatus` INT(11) NOT NULL COMMENT '优惠券状态',
  `WXCodeId` BIGINT(20) DEFAULT NULL COMMENT '微信Code记录号 与微信卡券投放记录关联',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `fk_couponrecord_couponid` (`CouponId`) USING BTREE,
  KEY `FK_couponrecord_shopid` (`ShopId`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=886 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_couponrecord`
--

LOCK TABLES `mall_couponrecord` WRITE;
/*!40000 ALTER TABLE `mall_couponrecord` DISABLE KEYS */;
INSERT INTO `mall_couponrecord` VALUES (884,60,'91e4eafb59c847bca7f89a5340d3c29a','2017-02-15 14:17:16','',0,NULL,NULL,1,'非凡资源店',0,NULL),(885,64,'1f0c3dff4a014d7da5b897e0e6589c0f','2017-02-16 21:15:32','',0,NULL,NULL,1,'官方自营店',0,NULL);
/*!40000 ALTER TABLE `mall_couponrecord` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_couponsendbyregister`
--

DROP TABLE IF EXISTS `mall_couponsendbyregister`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_couponsendbyregister` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT COMMENT '主键ID',
  `Status` INT(11) NOT NULL COMMENT '0、关闭；1、开启',
  `Link` VARCHAR(300) DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB DEFAULT CHARSET=utf8 COMMENT='注册赠送优惠券';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_couponsendbyregister`
--

LOCK TABLES `mall_couponsendbyregister` WRITE;
/*!40000 ALTER TABLE `mall_couponsendbyregister` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_couponsendbyregister` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_couponsendbyregisterdetailed`
--

DROP TABLE IF EXISTS `mall_couponsendbyregisterdetailed`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_couponsendbyregisterdetailed` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT COMMENT '主键ID',
  `CouponRegisterId` BIGINT(20) NOT NULL COMMENT '注册活动ID',
  `CouponId` BIGINT(20) NOT NULL COMMENT '优惠券ID',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Reference_z` (`CouponRegisterId`) USING BTREE,
  KEY `FK_Reference_coupon` (`CouponId`) USING BTREE
) ENGINE=INNODB DEFAULT CHARSET=utf8 COMMENT='注册送优惠券关联优惠券';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_couponsendbyregisterdetailed`
--

LOCK TABLES `mall_couponsendbyregisterdetailed` WRITE;
/*!40000 ALTER TABLE `mall_couponsendbyregisterdetailed` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_couponsendbyregisterdetailed` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_couponsetting`
--

DROP TABLE IF EXISTS `mall_couponsetting`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_couponsetting` (
  `Id` INT(11) NOT NULL AUTO_INCREMENT,
  `PlatForm` INT(11) NOT NULL COMMENT '优惠券的发行平台',
  `CouponID` BIGINT(20) NOT NULL,
  `Display` INT(11) NOT NULL COMMENT '是否显示',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=311 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_couponsetting`
--

LOCK TABLES `mall_couponsetting` WRITE;
/*!40000 ALTER TABLE `mall_couponsetting` DISABLE KEYS */;
INSERT INTO `mall_couponsetting` VALUES (59,0,62,1),(61,4,60,1),(62,0,60,1),(63,4,61,1),(64,0,61,1);
/*!40000 ALTER TABLE `mall_couponsetting` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_customerservice`
--

DROP TABLE IF EXISTS `mall_customerservice`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_customerservice` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `ShopId` BIGINT(20) NOT NULL,
  `Tool` INT(11) NOT NULL COMMENT '工具类型（QQ、旺旺）',
  `Type` INT(11) NOT NULL,
  `Name` VARCHAR(1000) NOT NULL COMMENT '客服名称',
  `AccountCode` VARCHAR(1000) NOT NULL COMMENT '通信账号',
  `TerminalType` INT(11) NOT NULL DEFAULT '0' COMMENT '终端类型',
  `ServerStatus` INT(11) NOT NULL DEFAULT '1' COMMENT '客服状态',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=73 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_customerservice`
--

LOCK TABLES `mall_customerservice` WRITE;
/*!40000 ALTER TABLE `mall_customerservice` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_customerservice` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_distributionbrokerage`
--

DROP TABLE IF EXISTS `mall_distributionbrokerage`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_distributionbrokerage` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT COMMENT '流水号',
  `OrderId` BIGINT(20) NOT NULL COMMENT '订单编号',
  `OrderItemId` BIGINT(20) NOT NULL COMMENT '订单项编号',
  `ProductId` BIGINT(20) NOT NULL COMMENT '商品编号',
  `MemberId` BIGINT(20) NOT NULL COMMENT '下单会员',
  `ShopId` BIGINT(20) NOT NULL COMMENT '店铺编号',
  `Quantity` BIGINT(20) NOT NULL COMMENT '购买数量',
  `RealPayAmount` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '实付金额',
  `BrokerageStatus` INT(11) NOT NULL COMMENT '佣金状态',
  `OrderDate` DATETIME DEFAULT NULL COMMENT '下单时间',
  `SettlementTime` DATETIME DEFAULT NULL COMMENT '结算时间',
  `SuperiorId1` BIGINT(20) NOT NULL DEFAULT '0' COMMENT '一级分销员',
  `BrokerageRate1` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '一级分佣比',
  `SuperiorId2` BIGINT(20) NOT NULL DEFAULT '0' COMMENT '二级分销员',
  `BrokerageRate2` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '二级分佣比',
  `SuperiorId3` BIGINT(20) NOT NULL DEFAULT '0' COMMENT '三级分销员',
  `BrokerageRate3` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '三级分佣比',
  PRIMARY KEY (`Id`)
) ENGINE=INNODB AUTO_INCREMENT=50 DEFAULT CHARSET=utf8 COMMENT='分销佣金表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_distributionbrokerage`
--

LOCK TABLES `mall_distributionbrokerage` WRITE;
/*!40000 ALTER TABLE `mall_distributionbrokerage` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_distributionbrokerage` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_distributionproduct`
--

DROP TABLE IF EXISTS `mall_distributionproduct`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_distributionproduct` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ProductId` BIGINT(20) NOT NULL COMMENT '商品编号',
  `ShopId` BIGINT(20) NOT NULL COMMENT '店铺编号',
  `ProductStatus` INT(11) NOT NULL COMMENT '商品分销状态',
  `BrokerageRate1` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '一级分佣比',
  `BrokerageRate2` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '二级分佣比',
  `BrokerageRate3` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '三级分佣比',
  `SaleCount` INT(11) NOT NULL DEFAULT '0' COMMENT '成交件数',
  `SaleAmount` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '成交金额',
  `SettlementAmount` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '已结算金额',
  `AddDate` DATETIME DEFAULT NULL COMMENT '添加推广时间',
  PRIMARY KEY (`Id`)
) ENGINE=INNODB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8 COMMENT='分销商品表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_distributionproduct`
--

LOCK TABLES `mall_distributionproduct` WRITE;
/*!40000 ALTER TABLE `mall_distributionproduct` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_distributionproduct` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_distributionranking`
--

DROP TABLE IF EXISTS `mall_distributionranking`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_distributionranking` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `BatchId` BIGINT(20) NOT NULL,
  `MemberId` BIGINT(20) NOT NULL COMMENT '销售员ID',
  `Quantity` INT(11) NOT NULL COMMENT '成交数量',
  `Amount` DECIMAL(10,2) NOT NULL COMMENT '成交金额',
  `Settlement` DECIMAL(10,2) NOT NULL COMMENT '已结算金额',
  `NoSettlement` DECIMAL(10,2) NOT NULL COMMENT '未结算金额',
  PRIMARY KEY (`Id`)
) ENGINE=INNODB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_distributionranking`
--

LOCK TABLES `mall_distributionranking` WRITE;
/*!40000 ALTER TABLE `mall_distributionranking` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_distributionranking` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_distributionrankingbatch`
--

DROP TABLE IF EXISTS `mall_distributionrankingbatch`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_distributionrankingbatch` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `BeginTime` DATETIME NOT NULL COMMENT '开始时间',
  `EndTime` DATETIME NOT NULL COMMENT '截止时间',
  `CreateTime` DATETIME NOT NULL COMMENT '创建时间',
  PRIMARY KEY (`Id`)
) ENGINE=INNODB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_distributionrankingbatch`
--

LOCK TABLES `mall_distributionrankingbatch` WRITE;
/*!40000 ALTER TABLE `mall_distributionrankingbatch` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_distributionrankingbatch` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_distributionshoprateconfig`
--

DROP TABLE IF EXISTS `mall_distributionshoprateconfig`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_distributionshoprateconfig` (
  `Id` INT(11) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ShopId` BIGINT(20) NOT NULL COMMENT '店铺编号',
  `BrokerageRate1` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '一级分佣比',
  `BrokerageRate2` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '二级分佣比',
  `BrokerageRate3` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '三级分佣比',
  PRIMARY KEY (`Id`)
) ENGINE=INNODB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8 COMMENT='分销默认佣金比例表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_distributionshoprateconfig`
--

LOCK TABLES `mall_distributionshoprateconfig` WRITE;
/*!40000 ALTER TABLE `mall_distributionshoprateconfig` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_distributionshoprateconfig` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_distributionwithdraw`
--

DROP TABLE IF EXISTS `mall_distributionwithdraw`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_distributionwithdraw` (
  `Id` BIGINT(20) NOT NULL COMMENT '流水号',
  `MemberId` BIGINT(20) NOT NULL COMMENT '会员ID',
  `WithdrawName` VARCHAR(100) DEFAULT NULL COMMENT '提现名',
  `WithdrawAccount` VARCHAR(100) DEFAULT NULL COMMENT '提现账号',
  `WithdrawStatus` INT(11) NOT NULL COMMENT '提现状态',
  `Amount` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '提现金额',
  `ApplyTime` DATETIME NOT NULL COMMENT '申请时间',
  `ConfirmTime` DATETIME DEFAULT NULL COMMENT '处理时间',
  `PayTime` DATETIME DEFAULT NULL COMMENT '付款时间',
  `PayNo` VARCHAR(50) DEFAULT NULL COMMENT '付款流水号',
  `Operator` VARCHAR(50) DEFAULT NULL COMMENT '操作人',
  `Remark` VARCHAR(200) DEFAULT NULL COMMENT '备注',
  `WithdrawType` INT(11) NOT NULL DEFAULT '0' COMMENT '提现方式',
  `Poundage` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '手续费',
  PRIMARY KEY (`Id`)
) ENGINE=INNODB DEFAULT CHARSET=utf8 COMMENT='分销提现';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_distributionwithdraw`
--

LOCK TABLES `mall_distributionwithdraw` WRITE;
/*!40000 ALTER TABLE `mall_distributionwithdraw` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_distributionwithdraw` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_distributor`
--

DROP TABLE IF EXISTS `mall_distributor`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_distributor` (
  `MemberId` BIGINT(20) NOT NULL COMMENT '编号',
  `SuperiorId` BIGINT(20) NOT NULL COMMENT '上级编号',
  `GradeId` BIGINT(20) NOT NULL DEFAULT '0' COMMENT '所属等级',
  `OrderCount` INT(11) NOT NULL DEFAULT '0' COMMENT '分销订单数',
  `ShopName` VARCHAR(50) DEFAULT NULL COMMENT '小店名称',
  `ShopLogo` VARCHAR(200) DEFAULT NULL COMMENT '小店图标',
  `IsShowShopLogo` BIT(1) NOT NULL DEFAULT b'1' COMMENT '是否展示小店logo',
  `DistributionStatus` INT(11) NOT NULL COMMENT '审核状态',
  `ApplyTime` DATETIME NOT NULL COMMENT '申请时间',
  `PassTime` DATETIME DEFAULT NULL COMMENT '通过时间',
  `Remark` VARCHAR(300) DEFAULT NULL COMMENT '备注',
  `SubNumber` INT(11) NOT NULL DEFAULT '0' COMMENT '直接下级数',
  `Balance` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '余额',
  `SettlementAmount` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '总结算收入',
  `FreezeAmount` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '冻结金额',
  `WithdrawalsAmount` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '已提现',
  `ProductCount` INT(11) NOT NULL DEFAULT '0' COMMENT '分销成交商品数',
  `SaleAmount` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '分销成交金额',
  `SubProductCount` INT(11) NOT NULL DEFAULT '0' COMMENT '下级分销成交商品数',
  `SubSaleAmount` DECIMAL(20,2) NOT NULL DEFAULT '0.00' COMMENT '下级分销成交金额',
  PRIMARY KEY (`MemberId`)
) ENGINE=INNODB DEFAULT CHARSET=utf8 COMMENT='分销用户表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_distributor`
--

LOCK TABLES `mall_distributor` WRITE;
/*!40000 ALTER TABLE `mall_distributor` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_distributor` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_distributorgrade`
--

DROP TABLE IF EXISTS `mall_distributorgrade`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_distributorgrade` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `GradeName` VARCHAR(20) NOT NULL COMMENT '名称',
  `Quota` DECIMAL(20,2) NOT NULL COMMENT '条件',
  PRIMARY KEY (`Id`)
) ENGINE=INNODB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8 COMMENT='分销员等级表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_distributorgrade`
--

LOCK TABLES `mall_distributorgrade` WRITE;
/*!40000 ALTER TABLE `mall_distributorgrade` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_distributorgrade` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_distributorrecord`
--

DROP TABLE IF EXISTS `mall_distributorrecord`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_distributorrecord` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `MemberId` BIGINT(20) NOT NULL COMMENT '分销员',
  `Type` TINYINT(4) NOT NULL COMMENT '流水类型',
  `Amount` DECIMAL(10,2) NOT NULL COMMENT '变更金额',
  `Balance` DECIMAL(10,2) NOT NULL COMMENT '变更后余额',
  `CreateTime` DATETIME NOT NULL COMMENT '创建时间',
  `Remark` VARCHAR(255) NOT NULL COMMENT '备注',
  PRIMARY KEY (`Id`)
) ENGINE=INNODB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_distributorrecord`
--

LOCK TABLES `mall_distributorrecord` WRITE;
/*!40000 ALTER TABLE `mall_distributorrecord` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_distributorrecord` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_expresselement`
--

DROP TABLE IF EXISTS `mall_expresselement`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_expresselement` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT COMMENT '主键',
  `ExpressId` BIGINT(20) NOT NULL COMMENT '快递公司ID',
  `ElementType` INT(11) NOT NULL COMMENT '元素类型',
  `LeftTopPointX` INT(11) NOT NULL COMMENT '面单元素X坐标1',
  `LeftTopPointY` INT(11) NOT NULL COMMENT '面单元素Y坐标1',
  `RightBottomPointX` INT(11) NOT NULL COMMENT '面单元素X坐标2',
  `RightBottomPointY` INT(11) NOT NULL COMMENT '面单元素Y坐标2',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=263 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_expresselement`
--

LOCK TABLES `mall_expresselement` WRITE;
/*!40000 ALTER TABLE `mall_expresselement` DISABLE KEYS */;
INSERT INTO `mall_expresselement` VALUES (94,20,1,4423,5312,5645,6104),(95,20,2,4274,3583,7534,4729),(96,20,3,6497,5270,7684,6208),(97,20,5,4562,2937,5714,3562),(98,20,6,5599,2958,6751,3583),(99,20,7,6451,2958,7603,3583),(100,20,8,1025,5333,2269,6229),(101,20,9,1036,2937,2188,3562),(102,20,10,2039,2937,3191,3562),(103,20,11,2972,2937,4124,3562),(104,20,12,817,3645,4055,4708),(105,20,13,2891,5416,4170,6270),(106,20,15,1336,6854,4112,7770),(107,20,19,4631,6145,7615,7520),(108,20,21,1520,4666,4158,5770),(109,19,21,1036,2645,2684,3395),(110,19,8,3006,2666,4274,3416),(111,19,12,967,3250,3894,4270),(112,19,13,1716,4145,3410,5020),(113,19,1,3029,4916,4182,5666),(114,19,5,898,5562,2050,6187),(115,19,6,1889,5562,3041,6187),(116,19,7,2891,5583,4043,6208),(117,19,2,610,6125,3894,7270),(118,19,3,1947,7020,3721,7770),(119,19,19,564,8020,3801,8916),(120,19,18,5979,9104,8052,10062),(121,19,15,541,8541,3847,9208),(122,21,8,1347,1770,2615,2645),(123,21,21,1244,2562,4481,3479),(124,21,9,1221,3312,2373,3937),(125,21,10,2235,3312,3387,3937),(126,21,11,3237,3312,4389,3937),(127,21,12,691,3854,4527,4854),(128,21,13,1566,4604,4078,5562),(129,21,19,656,6458,3099,7583),(130,21,15,3087,6687,5230,7520),(131,21,1,5345,1770,6774,2645),(132,21,5,5161,3312,6313,3937),(133,21,6,6232,3312,7384,3937),(134,21,7,7258,3312,8410,3937),(135,21,2,4735,3895,8605,4833),(136,21,3,5622,4583,8271,5458),(137,22,8,1566,2125,2811,2937),(138,22,9,1532,2833,2684,3458),(139,22,10,2523,2833,3675,3458),(140,22,11,3410,2812,4562,3437),(141,22,12,1497,3333,4550,4375),(142,22,21,1474,4104,4516,5104),(143,22,13,1520,4937,2949,5812),(144,22,1,5506,2125,6774,2895),(145,22,5,5437,2875,6589,3500),(146,22,6,6244,2895,7396,3520),(147,22,7,7246,2895,8398,3520),(148,22,2,5437,3395,8444,4708),(149,22,3,5483,4812,7062,5791),(150,22,19,2718,6208,4758,7312),(151,22,15,3421,5604,5794,6437),(152,22,18,4274,7375,5944,8395),(153,23,8,1255,1625,2442,2395),(154,23,21,1405,2270,4539,2979),(155,23,9,1059,3000,2211,3625),(156,23,10,2073,3000,3225,3625),(157,23,11,3122,3000,4274,3625),(158,23,12,898,3625,4585,4583),(159,23,13,1670,4500,3029,5312),(160,23,3,7534,1666,8824,2416),(161,23,5,5126,2833,6278,3458),(162,23,6,6186,2833,7338,3458),(163,23,7,7258,2812,8410,3437),(164,23,2,5080,3437,8640,4541),(165,23,1,5253,4500,6981,5375),(166,23,19,771,6125,4527,6937),(167,23,18,5311,8229,7246,8958),(168,23,15,2845,6541,5817,7645),(169,24,8,1463,2333,2661,3125),(170,24,13,3387,2333,4769,3166),(171,24,1,5564,2333,6751,3166),(172,24,3,7776,2312,9147,3187),(173,24,21,2039,3000,4700,3854),(174,24,9,1543,3625,2695,4250),(175,24,10,2546,3645,3698,4270),(176,24,11,3571,3625,4723,4250),(177,24,12,852,4145,4723,5041),(178,24,5,5691,3604,6843,4229),(179,24,6,6797,3604,7949,4229),(180,24,7,7891,3604,9043,4229),(181,24,2,4919,4145,9009,5145),(182,24,19,829,6333,4158,7395),(183,24,15,898,7125,3410,8270),(184,24,18,5426,7875,8986,8916),(185,25,8,1255,1895,2615,2645),(186,25,21,1186,2479,3986,3270),(187,25,9,1336,3062,2488,3687),(188,25,10,2281,3083,3433,3708),(189,25,11,3248,3083,4400,3708),(190,25,12,679,3645,4377,4895),(191,25,13,1693,4708,3421,5583),(192,25,19,610,6104,3041,7312),(193,25,15,610,6937,3548,7937),(194,25,18,6716,7479,8248,8750),(195,25,1,5057,1937,6520,2687),(196,25,5,5207,3062,6359,3687),(197,25,6,6163,3062,7315,3687),(198,25,7,7131,3083,8283,3708),(199,25,2,4458,3604,8317,5000),(200,25,3,5449,4625,6716,5500),(216,26,1,5633,2145,6831,2812),(217,26,2,4953,4395,8629,5791),(218,26,3,6394,5583,8594,6520),(219,26,5,4988,3770,6140,4395),(220,26,6,6117,3791,7269,4416),(221,26,7,7281,3812,8433,4437),(222,26,8,1658,1833,2868,2645),(223,26,9,990,3437,2142,4062),(224,26,10,2142,3458,3294,4083),(225,26,11,3248,3437,4400,4062),(226,26,12,898,4062,4677,5187),(227,26,13,2373,5145,4389,5937),(228,26,15,3640,5895,5864,6645),(229,26,19,1140,6208,4112,7416),(230,26,21,1923,2437,4205,3229),(231,26,18,3732,6562,5207,7500),(232,27,1,1451,3500,2753,4312),(233,27,3,3110,3500,4654,4395),(234,27,5,1785,4625,2937,5250),(235,27,6,2857,4666,4009,5291),(236,27,7,3859,4687,5011,5312),(237,27,2,1071,5145,4988,6041),(238,27,8,1509,5750,2753,6416),(239,27,13,3087,5750,4838,6562),(240,27,21,1774,6208,4723,7020),(241,27,9,1785,6854,2937,7479),(242,27,10,2811,6875,3963,7500),(243,27,11,3836,6854,4988,7479),(244,27,12,1002,7270,4792,8312),(245,27,19,4884,3479,6923,4520),(246,27,15,4884,4416,6935,5291),(247,28,8,1543,2458,2811,3354),(248,28,9,1209,3291,2361,3916),(249,28,10,2304,3270,3456,3895),(250,28,11,3398,3270,4550,3895),(251,28,12,668,3875,4631,4770),(252,28,21,1290,4541,4665,5291),(253,28,13,3444,5125,5046,5916),(254,28,18,5368,7895,7419,9062),(255,28,1,5806,2520,6993,3270),(256,28,5,5288,3312,6440,3937),(257,28,6,6347,3312,7500,3937),(258,28,7,7476,3291,8629,3916),(259,28,2,4884,3833,8755,5062),(260,28,3,7534,4979,8847,5791),(261,28,19,1670,5708,5264,7104),(262,28,15,1635,7416,5334,8166);
/*!40000 ALTER TABLE `mall_expresselement` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_expressinfo`
--

DROP TABLE IF EXISTS `mall_expressinfo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_expressinfo` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT COMMENT '主键',
  `Name` VARCHAR(50) NOT NULL COMMENT '快递名称',
  `TaobaoCode` VARCHAR(50) DEFAULT NULL COMMENT '淘宝编号',
  `Kuaidi100Code` VARCHAR(50) DEFAULT NULL COMMENT '快递100对应物流编号',
  `KuaidiNiaoCode` VARCHAR(50) DEFAULT NULL COMMENT '快递鸟物流公司编号',
  `Width` INT(11) NOT NULL COMMENT '快递面单宽度',
  `Height` INT(11) NOT NULL COMMENT '快递面单高度',
  `Logo` VARCHAR(100) DEFAULT NULL COMMENT '快递公司logo',
  `BackGroundImage` VARCHAR(100) DEFAULT NULL COMMENT '快递公司面单背景图片',
  `Status` INT(11) NOT NULL COMMENT '快递公司状态（0：正常，1：删除）',
  `CreateDate` DATETIME NOT NULL COMMENT '创建日期',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=29 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_expressinfo`
--

LOCK TABLES `mall_expressinfo` WRITE;
/*!40000 ALTER TABLE `mall_expressinfo` DISABLE KEYS */;
INSERT INTO `mall_expressinfo` VALUES (19,'顺丰速递',NULL,'shunfeng','SF',230,480,'/Storage/Plat/Express/logo/shunfeng.png','/Storage/Plat/Express/顺丰速递.png',1,'0001-01-01 00:00:00'),(20,'圆通速递',NULL,'yuantong','YTO',230,480,'/Storage/Plat/Express/logo/yuantong.png','/Storage/Plat/Express/圆通速递.png',1,'0001-01-01 00:00:00'),(21,'申通快递',NULL,'shentong','STO',230,480,'/Storage/Plat/Express/logo/shentong.png','/Storage/Plat/Express/申通快递.png',1,'0001-01-01 00:00:00'),(22,'中通快递',NULL,'zhongtong','ZTO',230,480,'/Storage/Plat/Express/logo/zhongtong.png','/Storage/Plat/Express/中通快递.png',1,'0001-01-01 00:00:00'),(23,'韵达速递',NULL,'yunda','YD',230,480,'/Storage/Plat/Express/logo/yunda.png','/Storage/Plat/Express/韵达速递.png',1,'0001-01-01 00:00:00'),(24,'EMS',NULL,'ems','EMS',230,480,'/Storage/Plat/Express/logo/ems.png','/Storage/Plat/Express/EMS.png',1,'0001-01-01 00:00:00'),(25,'百世汇通',NULL,'huitongkuaidi','HTKY',230,480,'/Storage/Plat/Express/logo/BaiShiHuiTong.png','/Storage/Plat/Express/百世汇通.png',1,'0001-01-01 00:00:00'),(26,'天天快递',NULL,'tiantian','HHTT',230,480,'/Storage/Plat/Express/logo/tiantian.png','/Storage/Plat/Express/天天快递.png',1,'0001-01-01 00:00:00'),(27,'邮政平邮',NULL,'youzhengguonei','YZPY',230,480,'','/Storage/Plat/Express/邮政平邮.png',1,'0001-01-01 00:00:00'),(28,'宅急送',NULL,'zhaijisong','ZJS',230,480,'/Storage/Plat/Express/logo/zhaijisong.png','/Storage/Plat/Express/宅急送.png',1,'0001-01-01 00:00:00');
/*!40000 ALTER TABLE `mall_expressinfo` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_favorite`
--

DROP TABLE IF EXISTS `mall_favorite`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_favorite` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `UserId` BIGINT(20) NOT NULL COMMENT '用户ID',
  `ProductId` BIGINT(20) NOT NULL,
  `Tags` VARCHAR(100) DEFAULT NULL COMMENT '分类标签',
  `Date` DATETIME NOT NULL COMMENT '收藏日期',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Member_Favorite` (`UserId`) USING BTREE,
  KEY `FK_Product_Favorite` (`ProductId`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=79 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_favorite`
--

LOCK TABLES `mall_favorite` WRITE;
/*!40000 ALTER TABLE `mall_favorite` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_favorite` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_favoriteshop`
--

DROP TABLE IF EXISTS `mall_favoriteshop`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_favoriteshop` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `UserId` BIGINT(20) NOT NULL COMMENT '用户ID',
  `ShopId` BIGINT(20) NOT NULL,
  `Tags` VARCHAR(100) DEFAULT NULL COMMENT '分类标签',
  `Date` DATETIME NOT NULL COMMENT '收藏日期',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `mall_FavoriteShop_fk_1` (`ShopId`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=196 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_favoriteshop`
--

LOCK TABLES `mall_favoriteshop` WRITE;
/*!40000 ALTER TABLE `mall_favoriteshop` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_favoriteshop` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_fightgroup`
--

DROP TABLE IF EXISTS `mall_fightgroup`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_fightgroup` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `HeadUserId` BIGINT(20) NOT NULL COMMENT '团长用户编号',
  `ActiveId` BIGINT(20) NOT NULL COMMENT '对应活动',
  `LimitedNumber` INT(11) NOT NULL COMMENT '参团人数限制',
  `LimitedHour` DECIMAL(18,2) NOT NULL COMMENT '时间限制',
  `JoinedNumber` INT(11) NOT NULL COMMENT '已参团人数',
  `IsException` BIT(1) NOT NULL COMMENT '是否异常',
  `GroupStatus` INT(11) NOT NULL COMMENT '数据状态 初始中  成团中  成功   失败',
  `AddGroupTime` DATETIME NOT NULL COMMENT '开团时间',
  `OverTime` DATETIME DEFAULT NULL COMMENT '结束时间 成功或失败的时间',
  `ProductId` BIGINT(20) NOT NULL DEFAULT '0' COMMENT '商品编号',
  `ShopId` BIGINT(20) NOT NULL DEFAULT '0' COMMENT '店铺编号',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8 COMMENT='拼团组团详情';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_fightgroup`
--

LOCK TABLES `mall_fightgroup` WRITE;
/*!40000 ALTER TABLE `mall_fightgroup` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_fightgroup` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_fightgroupactive`
--

DROP TABLE IF EXISTS `mall_fightgroupactive`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_fightgroupactive` (
  `Id` BIGINT(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ShopId` BIGINT(20) NOT NULL COMMENT '店铺编号',
  `ProductId` BIGINT(20) NOT NULL COMMENT '商品编号',
  `ProductName` VARCHAR(100) DEFAULT NULL COMMENT '商品名称',
  `IconUrl` VARCHAR(100) DEFAULT NULL COMMENT '图片',
  `StartTime` DATETIME NOT NULL COMMENT '开始时间',
  `EndTime` DATETIME NOT NULL COMMENT '结束时间',
  `LimitedNumber` INT(11) NOT NULL DEFAULT '0' COMMENT '参团人数限制',
  `LimitedHour` DECIMAL(18,2) NOT NULL DEFAULT '0.00' COMMENT '成团时限',
  `LimitQuantity` INT(11) NOT NULL DEFAULT '0' COMMENT '数量限制',
  `GroupCount` INT(11) NOT NULL DEFAULT '0' COMMENT '成团数量',
  `OkGroupCount` INT(11) NOT NULL DEFAULT '0' COMMENT '成功成团数量',
  `AddTime` DATETIME NOT NULL COMMENT '活动添加时间',
  `ManageAuditStatus` INT(11) NOT NULL DEFAULT '0' COMMENT '平台操作状态',
  `ManageRemark` VARCHAR(1000) DEFAULT NULL COMMENT '平台操作说明',
  `ManageDate` DATETIME DEFAULT NULL COMMENT '平台操作时间',
  `ManagerId` BIGINT(20) DEFAULT NULL COMMENT '平台操作人',
  `ActiveTimeStatus` INT(11) NOT NULL DEFAULT '0' COMMENT '活动当前状态',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=INNODB AUTO_INCREMENT=104 DEFAULT CHARSET=utf8 COMMENT='拼团活动';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_fightgroupactive`
--

LOCK TABLES `mall_fightgroupactive` WRITE;
/*!40000 ALTER TABLE `mall_fightgroupactive` DISABLE KEYS */;
INSERT INTO `mall_fightgroupactive` VALUES (26,1,718,'贝德玛（Bioderma）深层舒妍卸妆水 舒缓保湿粉水（干性中性敏感肌法国版/海外版随机发）500ml ','/Storage/Shop/1/Market/201702141131088043350.jpg','2017-02-14 11:29:33','2017-03-14 11:29:33',2,5.00,1,0,0,'2017-02-14 11:29:33',0,NULL,NULL,NULL,0),(27,1,714,'Apple MacBook Air 13.3英寸笔记本电脑 银色(Core i5 处理器/8GB内存/128GB SSD闪存 MMGF2CH/A)','/Storage/Shop/1/Market/201702141342584725700.jpg','2017-02-14 13:40:08','2017-04-30 13:40:08',15,48.00,1,0,0,'2017-02-14 11:29:33',0,NULL,NULL,NULL,0),(28,1,709,'ONLY2016春季新品T恤衬衫叠搭纯棉两件套连衣裙女T|116360504 10D炭花灰 165/84A/M','/Storage/Shop/1/Market/201702141347045918040.jpg','2017-02-14 13:45:08','2017-04-30 13:40:08',5,24.00,1,0,0,'2017-02-14 11:29:33',0,NULL,NULL,NULL,0),(29,1,702,'卫龙 休闲零食 辣条 小面筋 办公室休闲食品 22g*20包(新老包装随机发货)','/Storage/Shop/1/Market/201702141349148105050.jpg','2017-02-14 13:50:30','2017-03-14 13:45:30',2,5.00,5,0,0,'2017-02-14 11:29:33',0,NULL,NULL,NULL,0),(30,1,703,'卫龙 休闲零食 亲嘴条 辣条 400g/袋','/Storage/Shop/1/Market/201702141350510771510.jpg','2017-02-14 13:50:48','2017-03-14 13:49:48',2,5.00,5,0,0,'2017-02-14 11:29:33',0,NULL,NULL,NULL,0);
/*!40000 ALTER TABLE `mall_fightgroupactive` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_fightgroupactiveitem`
--

DROP TABLE IF EXISTS `mall_fightgroupactiveitem`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_fightgroupactiveitem` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ActiveId` bigint(20) NOT NULL COMMENT '所属活动',
  `ProductId` bigint(20) NOT NULL COMMENT '商品编号',
  `SkuId` varchar(100) DEFAULT NULL COMMENT '商品SKU',
  `ActivePrice` decimal(18,2) NOT NULL COMMENT '活动售价',
  `ActiveStock` int(20) NOT NULL COMMENT '活动库存',
  `BuyCount` int(11) NOT NULL DEFAULT '0' COMMENT '已售',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=340 DEFAULT CHARSET=utf8 COMMENT='拼团活动项';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_fightgroupactiveitem`
--

LOCK TABLES `mall_fightgroupactiveitem` WRITE;
/*!40000 ALTER TABLE `mall_fightgroupactiveitem` DISABLE KEYS */;
INSERT INTO `mall_fightgroupactiveitem` VALUES (159,26,718,'718_0_0_0',0.00,2000,0),(160,27,714,'714_0_0_0',6700.00,2000,0),(161,28,709,'709_0_0_0',450.00,150,0),(162,29,702,'702_0_0_0',10.00,1997,0),(163,30,703,'703_0_0_0',11.00,1998,0);
/*!40000 ALTER TABLE `mall_fightgroupactiveitem` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_fightgrouporder`
--

DROP TABLE IF EXISTS `mall_fightgrouporder`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_fightgrouporder` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ActiveId` bigint(20) NOT NULL COMMENT '对应活动',
  `ProductId` bigint(20) NOT NULL COMMENT '对应商品',
  `SkuId` varchar(100) DEFAULT NULL COMMENT '商品SKU',
  `GroupId` bigint(20) NOT NULL COMMENT '所属拼团',
  `OrderId` bigint(20) NOT NULL COMMENT '订单时间',
  `OrderUserId` bigint(20) NOT NULL COMMENT '订单用户编号',
  `IsFirstOrder` bit(1) NOT NULL COMMENT '是否团首订单',
  `JoinTime` datetime NOT NULL COMMENT '参团时间',
  `JoinStatus` int(11) NOT NULL COMMENT '参团状态 参团中  成功  失败',
  `OverTime` datetime DEFAULT NULL COMMENT '结束时间 成功或失败的时间',
  `Quantity` bigint(20) NOT NULL DEFAULT '0' COMMENT '购买数量',
  `SalePrice` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '销售价',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8 COMMENT='拼团订单';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_fightgrouporder`
--

LOCK TABLES `mall_fightgrouporder` WRITE;
/*!40000 ALTER TABLE `mall_fightgrouporder` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_fightgrouporder` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_flashsale`
--

DROP TABLE IF EXISTS `mall_flashsale`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_flashsale` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Title` varchar(30) NOT NULL,
  `ShopId` bigint(20) NOT NULL,
  `ProductId` bigint(20) NOT NULL,
  `Status` int(11) NOT NULL COMMENT '待审核,进行中,已结束,审核未通过,管理员取消',
  `BeginDate` datetime NOT NULL COMMENT '活动开始日期',
  `EndDate` datetime NOT NULL COMMENT '活动结束日期',
  `LimitCountOfThePeople` int(11) NOT NULL COMMENT '限制每人购买的数量',
  `SaleCount` int(11) NOT NULL COMMENT '仅仅只计算在限时购里的销售数',
  `CategoryName` varchar(255) NOT NULL,
  `ImagePath` varchar(255) NOT NULL,
  `MinPrice` decimal(18,2) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_FSShopId3` (`ShopId`) USING BTREE,
  KEY `FK_FSProductId3` (`ProductId`) USING BTREE,
  KEY `IX_ProductId_Status_BeginDate_EndDate` (`ProductId`,`Status`,`BeginDate`,`EndDate`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=198 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_flashsale`
--

LOCK TABLES `mall_flashsale` WRITE;
/*!40000 ALTER TABLE `mall_flashsale` DISABLE KEYS */;
INSERT INTO `mall_flashsale` VALUES (34,'限时折扣',1,717,5,'2017-02-14 11:40:00','2017-02-14 16:01:38',1,0,'洗面奶','/Storage/Shop/1/Products/717',17.00),(35,'限时折扣',1,715,5,'2017-02-14 11:41:00','2017-02-14 16:01:36',1,0,'电子产品','/Storage/Shop/1/Products/715',11688.00),(36,'限时折扣',1,713,5,'2017-02-14 11:43:00','2017-02-14 16:01:34',2,0,'服装','/Storage/Shop/1/Products/713',36.00),(37,'限时折扣',1,710,5,'2017-02-14 11:44:00','2017-02-14 16:01:32',1,0,'服装','/Storage/Shop/1/Products/710',200.00),(38,'限时折扣',1,712,5,'2017-02-15 15:35:00','2017-02-14 16:01:31',1,0,'服装','/Storage/Shop/1/Products/712',125.00),(39,'限时折扣',1,706,5,'2017-02-16 11:45:00','2017-02-14 16:01:29',1,0,'洗面奶','/Storage/Shop/1/Products/706',199.00),(40,'限时折扣',1,704,5,'2017-02-16 11:45:00','2017-02-14 16:02:35',1,0,'服装','/Storage/Shop/1/Products/704',388.00),(41,'限时折扣',1,700,5,'2017-02-18 14:45:00','2017-02-14 16:02:31',1,0,'食品','/Storage/Shop/1/Products/700',49.90),(42,'限时折扣',1,707,5,'2017-02-14 15:52:00','2017-02-14 16:01:24',2,0,'服装','/Storage/Shop/1/Products/707',150.00),(43,'限时折扣',1,707,2,'2017-02-14 16:03:00','2017-03-09 16:00:29',1,0,'服装','/Storage/Shop/1/Products/707',150.00),(44,'限时折扣',1,704,2,'2017-02-14 16:04:00','2017-03-13 16:00:29',1,0,'服装','/Storage/Shop/1/Products/704',399.00),(45,'限时折扣',1,705,2,'2017-02-14 16:05:00','2017-03-11 16:05:27',2,0,'服装','/Storage/Shop/1/Products/705',299.00),(46,'限时折扣',1,700,2,'2017-02-14 16:06:00','2017-03-11 16:05:16',1,0,'食品','/Storage/Shop/1/Products/700',40.00),(47,'限时折扣',1,699,2,'2017-02-14 16:06:00','2017-03-08 16:05:53',2,0,'食品','/Storage/Shop/1/Products/699',39.90),(48,'限时折扣',1,708,5,'2017-02-14 16:07:00','2017-02-14 16:24:11',2,0,'服装','/Storage/Shop/1/Products/708',399.00),(49,'限时折扣',1,741,2,'2017-02-14 16:20:00','2017-03-10 16:20:43',1,0,'电子产品','/Storage/Shop/1/Products/741',4999.00),(50,'限时折扣',1,736,2,'2017-02-14 16:21:00','2017-03-13 16:20:00',2,0,'洗面奶','/Storage/Shop/1/Products/736',19.90);
/*!40000 ALTER TABLE `mall_flashsale` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_flashsaleconfig`
--

DROP TABLE IF EXISTS `mall_flashsaleconfig`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_flashsaleconfig` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Preheat` int(11) NOT NULL COMMENT '预热时间',
  `IsNormalPurchase` tinyint(1) NOT NULL COMMENT '是否允许正常购买',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_flashsaleconfig`
--

LOCK TABLES `mall_flashsaleconfig` WRITE;
/*!40000 ALTER TABLE `mall_flashsaleconfig` DISABLE KEYS */;
INSERT INTO `mall_flashsaleconfig` VALUES (2,24,1);
/*!40000 ALTER TABLE `mall_flashsaleconfig` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_flashsaledetail`
--

DROP TABLE IF EXISTS `mall_flashsaledetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_flashsaledetail` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ProductId` bigint(20) NOT NULL,
  `SkuId` varchar(100) NOT NULL,
  `Price` decimal(18,2) NOT NULL COMMENT '限时购时金额',
  `TotalCount` int(11) NOT NULL DEFAULT '0' COMMENT '活动库存',
  `FlashSaleId` bigint(20) NOT NULL COMMENT '对应FlashSale表主键',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=576 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_flashsaledetail`
--

LOCK TABLES `mall_flashsaledetail` WRITE;
/*!40000 ALTER TABLE `mall_flashsaledetail` DISABLE KEYS */;
INSERT INTO `mall_flashsaledetail` VALUES (70,715,'715_0_0_0',11688.00,1,35),(71,717,'717_0_0_0',17.00,1,34),(72,713,'713_0_0_0',36.00,1,36),(73,710,'710_0_0_0',200.00,1,37),(74,712,'712_0_661_0',125.00,1,38),(75,712,'712_0_659_0',125.00,1,38),(76,706,'706_0_0_0',199.00,1,39),(77,704,'704_0_0_0',388.00,1,40),(78,700,'700_0_0_0',49.90,1,41),(79,707,'707_655_663_0',150.00,1,42),(80,707,'707_655_663_0',150.00,1,43),(81,704,'704_0_0_0',399.00,1,44),(82,705,'705_0_0_0',299.00,1,45),(83,700,'700_0_0_0',40.00,1,46),(84,699,'699_0_0_0',39.90,1,47),(85,708,'708_0_0_0',399.00,1,48),(86,741,'741_0_0_0',4999.00,1,49),(87,736,'736_0_0_0',19.90,1,50);
/*!40000 ALTER TABLE `mall_flashsaledetail` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_flashsaleremind`
--

DROP TABLE IF EXISTS `mall_flashsaleremind`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_flashsaleremind` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `OpenId` varchar(200) NOT NULL,
  `RecordDate` datetime NOT NULL,
  `FlashSaleId` bigint(20) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_flashsaleremind`
--

LOCK TABLES `mall_flashsaleremind` WRITE;
/*!40000 ALTER TABLE `mall_flashsaleremind` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_flashsaleremind` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_floorbrand`
--

DROP TABLE IF EXISTS `mall_floorbrand`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_floorbrand` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `FloorId` bigint(20) NOT NULL COMMENT '楼层ID',
  `BrandId` bigint(20) NOT NULL COMMENT '品牌ID',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Brand_FloorBrand` (`BrandId`) USING BTREE,
  KEY `FK_HomeFloor_FloorBrand` (`FloorId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=183 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_floorbrand`
--

LOCK TABLES `mall_floorbrand` WRITE;
/*!40000 ALTER TABLE `mall_floorbrand` DISABLE KEYS */;
INSERT INTO `mall_floorbrand` VALUES (133,154,360),(134,154,356),(135,154,355),(136,154,359),(138,153,359),(139,153,360),(140,153,356),(141,153,355),(142,163,356),(143,163,355),(144,163,352),(145,163,350),(146,163,336),(147,163,329),(148,163,360),(149,163,347),(150,163,353),(151,163,354),(152,163,332),(153,163,323),(154,163,338),(155,156,335),(156,156,331),(157,156,322),(158,156,329),(159,156,326),(160,156,338),(161,156,362),(162,156,344),(173,159,360),(174,159,347),(175,159,367),(176,159,339),(177,159,321),(178,159,324),(179,159,343),(180,159,334),(181,159,349),(182,159,364);
/*!40000 ALTER TABLE `mall_floorbrand` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_floorcategory`
--

DROP TABLE IF EXISTS `mall_floorcategory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_floorcategory` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `FloorId` bigint(20) NOT NULL,
  `CategoryId` bigint(20) NOT NULL,
  `Depth` int(11) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Category_FloorCategory` (`CategoryId`) USING BTREE,
  KEY `FK_HomeFloor_FloorCategory` (`FloorId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_floorcategory`
--

LOCK TABLES `mall_floorcategory` WRITE;
/*!40000 ALTER TABLE `mall_floorcategory` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_floorcategory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_floorproduct`
--

DROP TABLE IF EXISTS `mall_floorproduct`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_floorproduct` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `FloorId` bigint(20) NOT NULL COMMENT '楼层ID',
  `Tab` int(11) NOT NULL COMMENT '楼层标签',
  `ProductId` bigint(20) NOT NULL COMMENT '商品ID',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_HomeFloor_FloorProduct` (`FloorId`) USING BTREE,
  KEY `FK_Product_FloorProduct` (`ProductId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=381 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_floorproduct`
--

LOCK TABLES `mall_floorproduct` WRITE;
/*!40000 ALTER TABLE `mall_floorproduct` DISABLE KEYS */;
INSERT INTO `mall_floorproduct` VALUES (361,166,0,749),(362,166,0,746),(363,166,0,748),(364,166,0,745),(365,166,0,744),(366,166,0,743),(367,166,0,742),(368,166,0,741),(369,166,0,740),(370,166,0,739),(371,166,0,738),(372,166,0,737),(373,166,0,736),(374,166,0,734),(375,166,0,733),(376,167,0,749),(377,167,0,748),(378,167,0,747),(379,167,0,746),(380,167,0,745);
/*!40000 ALTER TABLE `mall_floorproduct` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_floortabl`
--

DROP TABLE IF EXISTS `mall_floortabl`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_floortabl` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `FloorId` bigint(20) NOT NULL COMMENT '楼层ID',
  `Name` varchar(50) NOT NULL COMMENT '楼层名称',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `Id` (`Id`) USING BTREE,
  KEY `FloorIdFK` (`FloorId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_floortabl`
--

LOCK TABLES `mall_floortabl` WRITE;
/*!40000 ALTER TABLE `mall_floortabl` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_floortabl` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_floortabldetail`
--

DROP TABLE IF EXISTS `mall_floortabldetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_floortabldetail` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `TabId` bigint(20) NOT NULL COMMENT 'TabID',
  `ProductId` bigint(20) NOT NULL COMMENT '商品ID',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `TabIdFK` (`TabId`) USING BTREE,
  KEY `ProductIdFK` (`ProductId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_floortabldetail`
--

LOCK TABLES `mall_floortabldetail` WRITE;
/*!40000 ALTER TABLE `mall_floortabldetail` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_floortabldetail` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_floortopic`
--

DROP TABLE IF EXISTS `mall_floortopic`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_floortopic` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `FloorId` bigint(20) NOT NULL COMMENT '楼层ID',
  `TopicType` int(11) NOT NULL COMMENT '专题类型',
  `TopicImage` varchar(100) NOT NULL COMMENT '专题封面图片',
  `TopicName` varchar(100) NOT NULL COMMENT '专题名称',
  `Url` varchar(1000) NOT NULL COMMENT '专题跳转URL',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_HomeFloor_FloorTopic` (`FloorId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=8099 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_floortopic`
--

LOCK TABLES `mall_floortopic` WRITE;
/*!40000 ALTER TABLE `mall_floortopic` DISABLE KEYS */;
INSERT INTO `mall_floortopic` VALUES (7832,151,17,'/Storage/Plat/PageSettings/HomeFloor/0/floor_BottomFive_20171227142817067566.jpg','','/'),(7833,151,18,'/Storage/Plat/PageSettings/HomeFloor/0/floor_18_20171227142817112646.jpg','','/'),(7834,151,19,'/Storage/Plat/PageSettings/HomeFloor/0/floor_19_20171227142817113435.jpg','','/'),(7835,151,20,'/Storage/Plat/PageSettings/HomeFloor/0/floor_20_20171227142817115389.jpg','','/'),(7836,151,21,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ROne_20171227142817116377.jpg','','/'),(7837,151,22,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RTwo_20171227142817117367.jpg','','/'),(7838,151,23,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RThree_20171227142817118453.jpg','','/'),(7839,151,24,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFour_20171227142817118453.jpg','','/'),(7840,152,21,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ROne_20171227150452023583.jpg','','/'),(7841,152,22,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RTwo_20171227150452024570.jpg','','/'),(7842,152,23,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RThree_20171227150452025541.jpg','','/'),(7843,152,24,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFour_20171227150452026521.jpg','','/'),(7844,152,25,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFive_20171227150452027492.jpg','','/'),(7845,152,26,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RSix_20171227150452028469.jpg','','/'),(7846,152,27,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RSeven_20171227150452028469.jpg','','/'),(7847,152,28,'/Storage/Plat/PageSettings/HomeFloor/0/floor_REight_20171227150452030438.jpg','','/'),(7848,152,29,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ScrollOne_20171227150452031413.jpg','','/'),(7849,152,30,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ScrollTwo_20171227150452031413.jpg','','/'),(7850,153,11,'','私人定制','/'),(7851,153,11,'','白金对戒','/'),(7852,153,11,'','男士经典','/'),(7853,153,11,'','女戒特卖','/'),(7854,153,11,'','成双成对','/'),(7855,153,11,'','女士经典','/'),(7856,153,11,'','年度推荐','/'),(7857,153,11,'','最受欢迎','/'),(7858,153,21,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ROne_20171227150627155464.jpg','','/'),(7859,153,22,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RTwo_20171227150627156423.jpg','','/'),(7860,153,23,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RThree_20171227150627174024.jpg','','/'),(7861,153,24,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFour_20171227150627174994.jpg','','/'),(7862,153,25,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFive_20171227150627175965.jpg','','/'),(7863,153,26,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RSix_20171227150627176948.jpg','','/'),(7864,153,27,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RSeven_20171227150627178873.jpg','','/'),(7865,153,28,'/Storage/Plat/PageSettings/HomeFloor/0/floor_REight_20171227150627179869.jpg','','/'),(7866,153,29,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ScrollOne_20171227150627179869.jpg','','/'),(7867,153,30,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ScrollTwo_20171227150627180841.jpg','','/'),(7868,154,11,'','海洋之星','/'),(7869,154,11,'','天使之钻','/'),(7870,154,11,'','钻石小鸟','/'),(7871,154,11,'','魔鬼之眼','/'),(7872,154,11,'','非洲之钻','/'),(7873,154,11,'','永恒之眼','/'),(7874,154,11,'','经典钻戒','/'),(7875,154,11,'','私人定制','/'),(7876,154,21,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ROne_20171227150718606607.jpg','','/'),(7877,154,22,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RTwo_20171227150718609541.jpg','','/'),(7878,154,23,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RThree_20171227150718610514.jpg','','/'),(7879,154,24,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFour_20171227150718611473.jpg','','/'),(7880,154,25,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFive_20171227150718612483.jpg','','/'),(7881,154,26,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RSix_20171227150718614407.jpg','','/'),(7882,154,27,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RSeven_20171227150718614407.jpg','','/'),(7883,154,28,'/Storage/Plat/PageSettings/HomeFloor/0/floor_REight_20171227150718615422.jpg','','/'),(7884,154,29,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ScrollOne_20171227150718615422.jpg','','/'),(7885,154,30,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ScrollTwo_20171227150718617355.jpg','','/'),(7886,155,17,'/Storage/Plat/PageSettings/HomeFloor/0/floor_BottomFive_20171227155631664379.jpg','','/'),(7887,155,18,'/Storage/Plat/PageSettings/HomeFloor/0/floor_18_20171227155631665362.jpg','','/'),(7888,155,19,'/Storage/Plat/PageSettings/HomeFloor/0/floor_19_20171227155631665362.jpg','','/'),(7889,155,20,'/Storage/Plat/PageSettings/HomeFloor/0/floor_20_20171227155631667370.jpg','','/'),(7890,155,21,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ROne_20171227155631668304.jpg','','/'),(7891,155,22,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RTwo_20171227155631669278.jpg','','/'),(7892,155,23,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RThree_20171227155631670249.jpg','','/'),(7893,155,24,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFour_20171227155631671248.jpg','','/'),(7904,157,21,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ROne_20171227160755116551.jpg','','/'),(7905,157,22,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RTwo_20171227160755118513.jpg','','/'),(7906,157,23,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RThree_20171227160755134127.jpg','','/'),(7907,157,24,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFour_20171227160755134127.jpg','','/'),(7908,157,25,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFive_20171227160755135107.jpg','','/'),(7909,157,26,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RSix_20171227160755136056.jpg','','/'),(7910,157,27,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RSeven_20171227160755137058.jpg','','/'),(7911,157,28,'/Storage/Plat/PageSettings/HomeFloor/0/floor_REight_20171227160755138031.jpg','','/'),(7912,157,29,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ScrollOne_20171227160755138031.jpg','','/'),(7913,157,30,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ScrollTwo_20171227160755139988.jpg','','/'),(7914,157,31,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ScrollThree_20171227160755140952.jpg','','/'),(7942,160,1,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftOne_20171227163433980988.jpg','','/'),(7943,160,21,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ROne_20171227163433981922.jpg','','/'),(7944,160,22,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RTwo_20171227163433981922.jpg','','/'),(7945,160,23,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RThree_20171227163433982893.jpg','','/'),(7946,160,24,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFour_20171227163433983866.jpg','','/'),(7947,160,25,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFive_20171227163433983866.jpg','','/'),(7948,161,33,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftUpOne_20171227163804755347.jpg','','/'),(7949,161,34,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftUpTwo_20171227163804757309.jpg','','/'),(7950,161,35,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftUpThree_20171227163804757309.jpg','','/'),(7951,161,36,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftUpFour_20171227163804758290.jpg','','/'),(7952,161,37,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftUpFive_20171227163804758290.jpg','','/'),(7953,161,38,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftUpSix_20171227163804759256.jpg','','/'),(7954,161,39,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftUpSeven_20171227163804759256.jpg','','/'),(7955,161,40,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftUpEight_20171227163804759256.jpg','','/'),(7956,161,41,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftUpNight_20171227163804760233.jpg','','/'),(7957,161,2,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftTwo_20171227163804761209.jpg','','/'),(7958,161,3,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleOne_20171227163804761209.jpg','','/'),(7959,161,4,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleTwo_20171227163804762192.jpg','','/'),(7960,161,5,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleThree_20171227163804763161.jpg','','/'),(7961,161,6,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleFour_20171227163804763161.jpg','','/'),(7962,161,7,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleFive_20171227163804764142.jpg','','/'),(7963,161,8,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RightOne_20171227163804764142.jpg','','/'),(7964,162,1,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftOne_20171227164023386231.jpg','','/'),(7965,162,3,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleOne_20171227164023388184.jpg','','/'),(7966,162,4,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleTwo_20171227164023389148.jpg','','/'),(7967,162,5,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleThree_20171227164023390133.jpg','','/'),(7968,162,6,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleFour_20171227164023391132.jpg','','/'),(7969,162,7,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleFive_20171227164023393043.jpg','','/'),(7970,162,12,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleSix_20171227164023393043.jpg','','/'),(7971,162,21,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ROne_20171227164023394024.jpg','','/'),(7972,162,22,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RTwo_20171227164023395012.jpg','','/'),(7973,162,23,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RThree_20171227164023395012.jpg','','/'),(7974,162,24,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFour_20171227164023396000.jpg','','/'),(7975,162,25,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFive_20171227164023396977.jpg','','/'),(7976,162,13,'/Storage/Plat/PageSettings/HomeFloor/0/floor_BottomOne_20171227164023396977.jpg','','/'),(7977,162,14,'/Storage/Plat/PageSettings/HomeFloor/0/floor_BottomTwo_20171227164023397943.jpg','','/'),(7978,162,15,'/Storage/Plat/PageSettings/HomeFloor/0/floor_BottomThree_20171227164023398917.jpg','','/'),(7981,163,1,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftOne_20171227164414089339.jpg','','/'),(7982,163,3,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleOne_20171227164414090325.jpg','','/'),(7983,164,1,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftOne_20171229115447118464.jpg','','/'),(7984,164,2,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftTwo_20171229115447120406.jpg','','/'),(7985,164,3,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleOne_20171229115447121380.jpg','','/'),(7986,164,4,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleTwo_20171229115447122323.jpg','','/'),(7987,164,5,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleThree_20171229115447123322.jpg','','/'),(7988,164,21,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ROne_20171229115447124296.jpg','','/'),(7989,164,22,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RTwo_20171229115447124296.jpg','','/'),(7990,164,23,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RThree_20171229115447126253.jpg','','/'),(7991,164,13,'/Storage/Plat/PageSettings/HomeFloor/0/floor_BottomOne_20171229115447127286.jpg','','/'),(7992,164,14,'/Storage/Plat/PageSettings/HomeFloor/0/floor_BottomTwo_20171229115447127286.jpg','','/'),(7993,164,15,'/Storage/Plat/PageSettings/HomeFloor/0/floor_BottomThree_20171229115447128201.jpg','','/'),(7994,164,16,'/Storage/Plat/PageSettings/HomeFloor/0/floor_BottomFour_20171229115447130155.jpg','','/'),(7995,164,17,'/Storage/Plat/PageSettings/HomeFloor/0/floor_BottomFive_20171229115447131135.jpg','','/'),(7996,165,17,'/Storage/Plat/PageSettings/HomeFloor/0/floor_BottomFive_20171229143628238594.jpg','','/'),(7997,165,18,'/Storage/Plat/PageSettings/HomeFloor/0/floor_18_20171229143628240538.jpg','','/'),(7998,165,19,'/Storage/Plat/PageSettings/HomeFloor/0/floor_19_20171229143628240538.jpg','','/'),(7999,165,20,'/Storage/Plat/PageSettings/HomeFloor/0/floor_20_20171229143628241542.jpg','','/'),(8000,165,21,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ROne_20171229143628242500.jpg','','/'),(8001,165,22,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RTwo_20171229143628243496.jpg','','/'),(8002,165,23,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RThree_20171229143628244426.jpg','','/'),(8003,165,24,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFour_20171229143628244426.jpg','','/'),(8004,158,21,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ROne_20171227162840012173.jpg','','/'),(8005,158,22,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RTwo_20171227162840013145.jpg','','/'),(8006,158,23,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RThree_20171227162840013145.jpg','','/'),(8007,158,24,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFour_20171227162840014120.jpg','','/'),(8008,158,25,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFive_20171227162840018030.jpg','','/'),(8009,158,26,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RSix_20171227162840019014.jpg','','/'),(8010,158,27,'/Storage/Plat/PageSettings/HomeFloor/158/floor_RSeven_20171227162927687965.jpg','','/'),(8011,158,28,'/Storage/Plat/PageSettings/HomeFloor/0/floor_REight_20171227162840019976.jpg','','/'),(8012,158,29,'/Storage/Plat/PageSettings/HomeFloor/158/floor_ScrollOne_20171227162927689907.jpg','','/'),(8013,158,30,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ScrollTwo_20171227162840021942.jpg','','/'),(8014,158,31,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ScrollThree_20171227162840021942.jpg','','/'),(8025,156,1,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftOne_20171227160413692707.jpg','','/'),(8026,156,2,'/Storage/Plat/PageSettings/HomeFloor/0/floor_LeftTwo_20171227160413695649.jpg','','/'),(8027,156,3,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleOne_20171227160413697593.jpg','','/'),(8028,156,4,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleTwo_20171227160413698571.jpg','','/'),(8029,156,5,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleThree_20171227160413698571.jpg','','/'),(8030,156,6,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleFour_20171227160413699557.jpg','','/'),(8031,156,7,'/Storage/Plat/PageSettings/HomeFloor/0/floor_MiddleFive_20171227160413700541.jpg','','/'),(8032,156,8,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RightOne_20171227160413701487.jpg','','/'),(8033,156,9,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RightTwo_20171227160413701487.jpg','','/'),(8034,156,10,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RightThree_20171227160413702482.jpg','','/'),(8083,159,11,'','女鞋','/'),(8084,159,11,'','男鞋','/'),(8085,159,11,'','雪地靴','/'),(8086,159,11,'','长筒靴','/'),(8087,159,17,'/Storage/Plat/PageSettings/HomeFloor/0/floor_BottomFive_20171227163304667459.jpg','','/'),(8088,159,18,'/Storage/Plat/PageSettings/HomeFloor/0/floor_18_20171227163304668443.jpg','','/'),(8089,159,19,'/Storage/Plat/PageSettings/HomeFloor/0/floor_19_20171227163304669423.jpg','','/'),(8090,159,20,'/Storage/Plat/PageSettings/HomeFloor/0/floor_20_20171227163304669423.jpg','','/'),(8091,159,18,'/Storage/Plat/PageSettings/HomeFloor/0/floor_18_20171227163304671373.jpg','','/'),(8092,159,19,'/Storage/Plat/PageSettings/HomeFloor/0/floor_19_20171227163304671373.jpg','','/'),(8093,159,20,'/Storage/Plat/PageSettings/HomeFloor/0/floor_20_20171227163304672351.jpg','','/'),(8094,159,21,'/Storage/Plat/PageSettings/HomeFloor/0/floor_ROne_20171227163304674319.jpg','','/'),(8095,159,22,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RTwo_20171227163304675283.jpg','','/'),(8096,159,23,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RThree_20171227163304675283.jpg','','/'),(8097,159,24,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFour_20171227163304676258.jpg','','/'),(8098,159,25,'/Storage/Plat/PageSettings/HomeFloor/0/floor_RFive_20171227163304676258.jpg','','/');
/*!40000 ALTER TABLE `mall_floortopic` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_freightareacontent`
--

DROP TABLE IF EXISTS `mall_freightareacontent`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_freightareacontent` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `FreightTemplateId` bigint(20) NOT NULL COMMENT '运费模板ID',
  `AreaContent` varchar(4000) DEFAULT NULL COMMENT '地区选择',
  `FirstUnit` int(11) NOT NULL COMMENT '首笔单元计量',
  `FirstUnitMonry` float NOT NULL COMMENT '首笔单元费用',
  `AccumulationUnit` int(11) NOT NULL COMMENT '递增单元计量',
  `AccumulationUnitMoney` float NOT NULL COMMENT '递增单元费用',
  `IsDefault` tinyint(4) NOT NULL COMMENT '是否为默认',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Freighttemalate_FreightAreaContent` (`FreightTemplateId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=596 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_freightareacontent`
--

LOCK TABLES `mall_freightareacontent` WRITE;
/*!40000 ALTER TABLE `mall_freightareacontent` DISABLE KEYS */;
INSERT INTO `mall_freightareacontent` VALUES (351,166,NULL,1,0,1,0,1),(353,168,NULL,1,8,1,0,1),(354,169,NULL,1,20,1,0,1),(355,167,NULL,1,10,1,0,1),(356,167,NULL,1,15,1,2,0);
/*!40000 ALTER TABLE `mall_freightareacontent` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_freightareadetail`
--

DROP TABLE IF EXISTS `mall_freightareadetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_freightareadetail` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `FreightTemplateId` bigint(20) NOT NULL COMMENT '运费模板ID',
  `FreightAreaId` bigint(20) NOT NULL COMMENT '模板地区Id',
  `ProvinceId` int(20) NOT NULL COMMENT '省份ID',
  `CityId` int(20) NOT NULL COMMENT '城市ID',
  `CountyId` int(20) NOT NULL COMMENT '区ID',
  `TownIds` varchar(2000) DEFAULT '' COMMENT '乡镇的ID用逗号隔开',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=675 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_freightareadetail`
--

LOCK TABLES `mall_freightareadetail` WRITE;
/*!40000 ALTER TABLE `mall_freightareadetail` DISABLE KEYS */;
INSERT INTO `mall_freightareadetail` VALUES (393,167,356,1812,1813,1819,'27148,27154');
/*!40000 ALTER TABLE `mall_freightareadetail` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_freighttemplate`
--

DROP TABLE IF EXISTS `mall_freighttemplate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_freighttemplate` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) DEFAULT NULL COMMENT '运费模板名称',
  `SourceAddress` int(11) NOT NULL COMMENT '宝贝发货地',
  `SendTime` varchar(100) DEFAULT NULL COMMENT '发送时间',
  `IsFree` int(11) NOT NULL COMMENT '是否商家负责运费',
  `ValuationMethod` int(11) NOT NULL COMMENT '定价方法(按体积、重量计算）',
  `ShippingMethod` int(11) DEFAULT NULL COMMENT '运送类型（物流、快递）',
  `ShopID` bigint(20) NOT NULL COMMENT '店铺ID',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=271 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_freighttemplate`
--

LOCK TABLES `mall_freighttemplate` WRITE;
/*!40000 ALTER TABLE `mall_freighttemplate` DISABLE KEYS */;
INSERT INTO `mall_freighttemplate` VALUES (166,'免运费',3685,NULL,1,0,NULL,1),(167,'10元运费',3984,'24',0,0,NULL,1),(168,'8元运费',14136,'12',0,0,NULL,1),(169,'20元运费',9509,'48',0,0,NULL,1);
/*!40000 ALTER TABLE `mall_freighttemplate` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_fulldiscountrule`
--

DROP TABLE IF EXISTS `mall_fulldiscountrule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_fulldiscountrule` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ActiveId` bigint(20) NOT NULL COMMENT '活动编号',
  `Quota` decimal(18,2) NOT NULL COMMENT '条件',
  `Discount` decimal(18,2) NOT NULL COMMENT '优惠',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `IDX_mall_Fules_ActiveId` (`ActiveId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=665 DEFAULT CHARSET=utf8 COMMENT='满减规则';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_fulldiscountrule`
--

LOCK TABLES `mall_fulldiscountrule` WRITE;
/*!40000 ALTER TABLE `mall_fulldiscountrule` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_fulldiscountrule` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_gift`
--

DROP TABLE IF EXISTS `mall_gift`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_gift` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `GiftName` varchar(100) NOT NULL COMMENT '名称',
  `NeedIntegral` int(11) NOT NULL COMMENT '需要积分',
  `LimtQuantity` int(11) NOT NULL COMMENT '限制兑换数量 0表示不限兑换数量',
  `StockQuantity` int(11) NOT NULL COMMENT '库存数量',
  `EndDate` datetime NOT NULL COMMENT '兑换结束时间',
  `NeedGrade` int(11) NOT NULL DEFAULT '0' COMMENT '等级要求 0表示不限定',
  `VirtualSales` int(11) NOT NULL DEFAULT '0' COMMENT '虚拟销量',
  `RealSales` int(11) NOT NULL DEFAULT '0' COMMENT '实际销量',
  `SalesStatus` int(11) NOT NULL COMMENT '状态',
  `ImagePath` varchar(100) DEFAULT NULL COMMENT '图片存放地址',
  `Sequence` int(11) NOT NULL DEFAULT '100' COMMENT '顺序 默认100 数字越小越靠前',
  `GiftValue` decimal(8,2) NOT NULL COMMENT '礼品价值',
  `Description` longtext COMMENT '描述',
  `AddDate` datetime NOT NULL COMMENT '添加时间',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=80 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_gift`
--

LOCK TABLES `mall_gift` WRITE;
/*!40000 ALTER TABLE `mall_gift` DISABLE KEYS */;
INSERT INTO `mall_gift` VALUES (68,'巧克力',520,1,520,'2017-03-14 10:39:44',0,0,0,1,'/Storage/Gift/68',100,52.00,'<p><span style=\"color: rgb(51, 51, 51); font-family: arial, sans-serif; font-size: 14px; line-height: 24px; text-indent: 28px; background-color: rgb(255, 255, 255);\">巧克力是一个外来词Chocolate的译音(粤港澳粤语地区译为“朱古力”）。主原料是可可豆（像椰子般的果实，在树干上会开花结果），它的起源甚早，始于墨西哥极盛一时的阿斯帝卡王朝最后一任皇帝孟特儒 ，当时是崇拜巧克力的社会，喜欢以辣椒、番椒、香草豆和香料添加在饮料中，打起泡沫，并以黄金杯子每天喝50CC，是属于宫廷成员的饮料，它的学名Theobroma有“众神的饮料”之意，被视为贵重的强心、利尿的药剂，它对胃液中的蛋白质分解酵素具有活化性的作用，可帮助消化。</span></p>','2017-02-14 10:44:15'),(69,'安踏女鞋跑步鞋减震气垫鞋',3000,1,100,'2017-06-30 22:55:29',7,0,0,1,'/Storage/Gift/69',100,449.00,'<p><img src=\"/Storage/Gift/69/Details/c9915ac7a8cb42269a3e25cdbe2f98bb.jpg\" title=\"386929711956251254970900_x.jpg\" alt=\"386929711956251254970900_x.jpg\"/><img src=\"/Storage/Gift/69/Details/10104b8542d04420b18a2432cffa1753.jpg\" title=\"164976342221393120792927_x.jpg\" alt=\"164976342221393120792927_x.jpg\"/></p>','2017-02-16 11:27:40'),(70,'珀莱雅水漾肌密细肤水 深层补水保湿爽肤水二次清洁收缩毛孔',2500,1,100,'2017-08-11 10:50:44',7,0,0,1,'/Storage/Gift/70',100,136.00,'<p><img src=\"/Storage/Gift/70/Details/18a8a48c548b4b4d857629778a87ae70.jpg\" title=\"133241810015006204522358_x.jpg\" alt=\"133241810015006204522358_x.jpg\"/><img src=\"/Storage/Gift/70/Details/04f07f15f245499b812f1789019006d0.jpg\" title=\"890340736812360889207700_x.jpg\" alt=\"890340736812360889207700_x.jpg\"/></p>','2017-02-16 11:31:05'),(71,'捷昇 电镀哑铃 10KG 15KG 20KG 30KG 40KG （2只) 健身电镀哑铃 家用健身器材 可拆装组合 10KG一对/5公斤X2只/赠30CM接杆',1000,1,100,'2017-08-12 11:30:08',6,0,0,1,'/Storage/Gift/71',100,468.00,'<p><img src=\"/Storage/Gift/71/Details/003e888e9a194399a1b2774d6971a109.jpg\" title=\"154086736697115606835300_x.jpg\" alt=\"154086736697115606835300_x.jpg\"/></p>','2017-02-16 11:34:28'),(72,'变白变美变年轻',500,1,100,'2017-08-12 11:35:11',6,0,0,1,'/Storage/Gift/72',100,36.00,'<p>暂无</p>','2017-02-16 11:36:13'),(73,'蒙牛真果粒牛奶(芦荟果粒)250ml*12盒',1200,1,100,'2017-08-12 11:35:17',6,0,0,1,'/Storage/Gift/73',100,42.00,'<p><img src=\"/Storage/Gift/73/Details/141c9df0b9e444b5b4e03279e70ffaa6.jpg\" title=\"104626455028760745576600_x.jpg\" alt=\"104626455028760745576600_x.jpg\"/><img src=\"/Storage/Gift/73/Details/516b0c9e58d2479683bb81d6aa06a246.jpg\" title=\"186009738717751867866643_x.jpg\" alt=\"186009738717751867866643_x.jpg\"/></p>','2017-02-16 11:38:23'),(74,'蒙牛真果粒牛奶(芦荟果粒)250ml*12盒',1200,1,100,'2017-08-12 11:35:17',6,0,0,1,'/Storage/Gift/74',100,42.00,'<p><img src=\"/Storage/Gift/74/Details/96481a531d354a0d8a097de0478f8e1d.jpg\" title=\"104626455028760745576600_x.jpg\" alt=\"104626455028760745576600_x.jpg\"/><img src=\"/Storage/Gift/74/Details/e14d9533de374f8f8347e9976cddd65b.jpg\" title=\"186009738717751867866643_x.jpg\" alt=\"186009738717751867866643_x.jpg\"/></p>','2017-02-16 11:38:30'),(75,'【卿卿雨】500克×3袋新疆四星和田枣超五星 红枣 新疆枣',900,1,100,'2017-11-04 11:35:37',6,0,0,1,'/Storage/Gift/75',100,132.00,'<p><img src=\"/Storage/Gift/75/Details/8be13388a25f4dc9a4d677e9348bed4e.jpg\" title=\"201404300347464392_x (1).jpg\" alt=\"201404300347464392_x (1).jpg\"/><img src=\"/Storage/Gift/75/Details/a8a9c9ce40bf49f692c7eb4b61fd372c.jpg\" title=\"201404300347572008_x.jpg\" alt=\"201404300347572008_x.jpg\"/></p>','2017-02-16 11:40:49'),(76,'澳洲Healthy Care金装蜂胶软胶囊 200粒/瓶 提高免疫美白养颜 海外原装进口',2200,1,100,'2017-10-07 11:10:13',6,100,0,1,'/Storage/Gift/76',100,399.00,'<p><img src=\"/Storage/Gift/76/Details/6131887fe26d4e17b4267759a2a5c89a.jpg\" title=\"212553520786951466781620_x.jpg\" alt=\"212553520786951466781620_x.jpg\"/><img src=\"/Storage/Gift/76/Details/cbe575eaebd34e29980e76012b513564.jpg\" title=\"385721060193940684290250_x.jpg\" alt=\"385721060193940684290250_x.jpg\"/></p>','2017-02-16 11:42:58'),(77,'【松桂坊_五花腊肉500*2袋】 湖南湘西农家传统工艺 土猪特产腊肠腊肉烟熏腊肉',3000,1,100,'2017-09-09 11:55:06',7,0,0,1,'/Storage/Gift/77',100,88.00,'<p><img src=\"/Storage/Gift/77/Details/c8b2af53b85349b5af4056ab85b73629.jpg\" title=\"403519394375682081332700_x.jpg\" alt=\"403519394375682081332700_x.jpg\"/><img src=\"/Storage/Gift/77/Details/2d9cef803baa43ad801463f28abb74d4.jpg\" title=\"132029199783667016411770_x.jpg\" alt=\"132029199783667016411770_x.jpg\"/></p>','2017-02-16 11:45:13');
/*!40000 ALTER TABLE `mall_gift` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_giftorder`
--

DROP TABLE IF EXISTS `mall_giftorder`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_giftorder` (
  `Id` bigint(20) NOT NULL COMMENT '编号',
  `OrderStatus` int(11) NOT NULL COMMENT '订单状态',
  `UserId` bigint(20) NOT NULL COMMENT '用户编号',
  `UserRemark` varchar(200) DEFAULT NULL COMMENT '会员留言',
  `ShipTo` varchar(100) DEFAULT NULL COMMENT '收货人',
  `CellPhone` varchar(100) DEFAULT NULL COMMENT '收货人电话',
  `TopRegionId` int(11) NOT NULL COMMENT '一级地区',
  `RegionId` int(11) NOT NULL COMMENT '地区编号',
  `RegionFullName` varchar(100) DEFAULT NULL COMMENT '地区全称',
  `Address` varchar(100) DEFAULT NULL COMMENT '地址',
  `ExpressCompanyName` varchar(4000) DEFAULT NULL COMMENT '快递公司',
  `ShipOrderNumber` varchar(4000) DEFAULT NULL COMMENT '快递单号',
  `ShippingDate` datetime DEFAULT NULL COMMENT '发货时间',
  `OrderDate` datetime NOT NULL COMMENT '下单时间',
  `FinishDate` datetime DEFAULT NULL COMMENT '完成时间',
  `TotalIntegral` int(11) NOT NULL COMMENT '积分总价',
  `CloseReason` varchar(200) DEFAULT NULL COMMENT '关闭原因',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_giftorder`
--

LOCK TABLES `mall_giftorder` WRITE;
/*!40000 ALTER TABLE `mall_giftorder` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_giftorder` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_giftorderitem`
--

DROP TABLE IF EXISTS `mall_giftorderitem`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_giftorderitem` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `OrderId` bigint(20) NOT NULL COMMENT '订单编号',
  `GiftId` bigint(20) NOT NULL COMMENT '礼品编号',
  `Quantity` int(11) NOT NULL COMMENT '数量',
  `SaleIntegral` int(11) NOT NULL COMMENT '积分单价',
  `GiftName` varchar(100) DEFAULT NULL COMMENT '礼品名称',
  `GiftValue` decimal(8,3) NOT NULL COMMENT '礼品价值',
  `ImagePath` varchar(100) DEFAULT NULL COMMENT '图片存放地址',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_mall_Gitem_OrderId` (`OrderId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=26 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_giftorderitem`
--

LOCK TABLES `mall_giftorderitem` WRITE;
/*!40000 ALTER TABLE `mall_giftorderitem` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_giftorderitem` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_handslidead`
--

DROP TABLE IF EXISTS `mall_handslidead`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_handslidead` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ImageUrl` varchar(100) NOT NULL COMMENT '图片URL',
  `Url` varchar(1000) NOT NULL COMMENT '图片跳转URL',
  `DisplaySequence` bigint(20) NOT NULL COMMENT '排序',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_handslidead`
--

LOCK TABLES `mall_handslidead` WRITE;
/*!40000 ALTER TABLE `mall_handslidead` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_handslidead` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_homecategory`
--

DROP TABLE IF EXISTS `mall_homecategory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_homecategory` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `RowId` int(11) NOT NULL COMMENT '分类所属行数',
  `CategoryId` bigint(20) NOT NULL COMMENT '分类ID',
  `Depth` int(11) NOT NULL COMMENT '分类深度(最深3）',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Category_HomeCategory` (`CategoryId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2519 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_homecategory`
--

LOCK TABLES `mall_homecategory` WRITE;
/*!40000 ALTER TABLE `mall_homecategory` DISABLE KEYS */;
INSERT INTO `mall_homecategory` VALUES (2450,1,1,1),(2451,1,3,3),(2452,1,6,3),(2453,2,8,1),(2454,2,11,3),(2455,2,12,3),(2456,2,13,3),(2457,3,14,1),(2458,3,16,3),(2459,4,17,1),(2460,4,19,3),(2461,5,20,1),(2462,5,22,3),(2463,5,23,3),(2464,5,157,3),(2465,6,25,1),(2466,6,28,3),(2467,6,32,3),(2468,6,33,3),(2469,6,36,3),(2470,6,37,3),(2471,6,38,3),(2472,7,39,1),(2473,7,72,3),(2474,7,73,3),(2475,7,75,3),(2476,7,76,3),(2477,7,79,3),(2478,7,81,3),(2479,7,86,3),(2480,7,87,3),(2481,8,40,1),(2482,8,93,3),(2483,8,97,3),(2484,8,98,3),(2485,8,99,3),(2486,8,102,3),(2487,8,103,3),(2488,8,106,3),(2493,15,152,1),(2494,14,151,1),(2495,14,159,3),(2496,13,45,1),(2497,13,143,3),(2498,13,144,3),(2499,13,145,3),(2500,13,147,3),(2501,13,148,3),(2502,13,149,3),(2503,12,44,1),(2504,12,134,3),(2505,12,135,3),(2506,12,136,3),(2507,12,137,3),(2508,12,156,3),(2509,11,43,1),(2510,10,42,1),(2511,10,128,3),(2512,10,130,3),(2513,10,131,3),(2514,9,24,1),(2515,9,54,3),(2516,9,55,3),(2517,9,58,3),(2518,9,59,3);
/*!40000 ALTER TABLE `mall_homecategory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_homecategoryrow`
--

DROP TABLE IF EXISTS `mall_homecategoryrow`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_homecategoryrow` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `RowId` int(11) NOT NULL COMMENT '行ID',
  `Image1` varchar(100) NOT NULL COMMENT '所属行推荐图片1',
  `Url1` varchar(100) NOT NULL COMMENT '所属行推荐图片1的URL',
  `Image2` varchar(100) NOT NULL COMMENT '所属行推荐图片2',
  `Url2` varchar(100) NOT NULL COMMENT '所属行推荐图片2的URL',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_homecategoryrow`
--

LOCK TABLES `mall_homecategoryrow` WRITE;
/*!40000 ALTER TABLE `mall_homecategoryrow` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_homecategoryrow` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_homefloor`
--

DROP TABLE IF EXISTS `mall_homefloor`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_homefloor` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `FloorName` varchar(100) NOT NULL COMMENT '楼层名称',
  `SubName` varchar(100) DEFAULT NULL COMMENT '楼层小标题',
  `DisplaySequence` bigint(20) NOT NULL COMMENT '显示顺序',
  `IsShow` tinyint(1) NOT NULL COMMENT '是否显示的首页',
  `StyleLevel` int(10) unsigned NOT NULL COMMENT '楼层所属样式（目前支持2套）',
  `DefaultTabName` varchar(50) DEFAULT NULL COMMENT '楼层的默认tab标题',
  `CommodityStyle` int(11) NOT NULL COMMENT '商品样式，0：默认，1：一排5个，2：一排4个',
  `DisplayMode` int(11) NOT NULL COMMENT '显示方式，0=默认，1=平铺展示，2=左右轮播',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `Id` (`Id`),
  KEY `Id_2` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=168 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_homefloor`
--

LOCK TABLES `mall_homefloor` WRITE;
/*!40000 ALTER TABLE `mall_homefloor` DISABLE KEYS */;
INSERT INTO `mall_homefloor` VALUES (151,'',NULL,1,1,12,NULL,0,0),(152,'黄金饰品',NULL,2,1,4,'黄金饰品',0,0),(153,'奢华白金',NULL,3,1,4,'奢华白金',0,0),(154,'璀璨钻石',NULL,4,1,4,'璀璨钻石',0,0),(155,'',NULL,5,1,12,NULL,0,0),(156,'数码相机','',6,1,3,NULL,0,0),(157,'电脑办公',NULL,7,1,1,'电脑办公',0,0),(158,'酒类食品',NULL,8,1,1,'热销推荐',0,0),(159,'潮流服饰',NULL,9,1,5,'热卖服饰',0,0),(160,'进口手表','原装正品',10,1,0,NULL,0,0),(161,'休闲零食',NULL,11,1,6,'热卖',0,0),(162,'护肤美白',NULL,12,1,7,'热卖',0,0),(163,'',NULL,13,1,3,NULL,0,0),(164,'户外运动',NULL,14,1,8,'热卖',0,0),(165,'',NULL,15,1,12,NULL,0,0),(166,'为您推荐',NULL,16,1,10,NULL,1,1),(167,'我们的额啊',NULL,17,1,10,NULL,1,2);
/*!40000 ALTER TABLE `mall_homefloor` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_imagead`
--

DROP TABLE IF EXISTS `mall_imagead`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_imagead` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL,
  `ImageUrl` varchar(100) NOT NULL COMMENT '图片的存放URL',
  `Url` varchar(1000) NOT NULL COMMENT '图片的调整地址',
  `IsTransverseAD` tinyint(1) NOT NULL COMMENT '是否是横向长广告',
  `TypeId` int(11) NOT NULL DEFAULT '0' COMMENT '微信头像',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=7515 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_imagead`
--

LOCK TABLES `mall_imagead` WRITE;
/*!40000 ALTER TABLE `mall_imagead` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_imagead` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_integralmallad`
--

DROP TABLE IF EXISTS `mall_integralmallad`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_integralmallad` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ActivityType` int(11) NOT NULL COMMENT '活动类型',
  `ActivityId` bigint(20) NOT NULL COMMENT '活动编号',
  `Cover` varchar(255) DEFAULT NULL COMMENT '显示图片',
  `ShowStatus` int(11) NOT NULL COMMENT '显示状态',
  `ShowPlatform` int(11) NOT NULL COMMENT '显示平台',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_integralmallad`
--

LOCK TABLES `mall_integralmallad` WRITE;
/*!40000 ALTER TABLE `mall_integralmallad` DISABLE KEYS */;
INSERT INTO `mall_integralmallad` VALUES (3,2,155,'/Storage/Plat/WeiActivity/WeiActivity_20170214105428946248.jpg',1,2),(4,1,154,'/Storage/Plat/WeiActivity/WeiActivity_20170214104941480463.jpg',1,2);
/*!40000 ALTER TABLE `mall_integralmallad` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_inviterecord`
--

DROP TABLE IF EXISTS `mall_inviterecord`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_inviterecord` (
  `Id` bigint(11) NOT NULL AUTO_INCREMENT,
  `UserName` varchar(100) NOT NULL COMMENT '用户名',
  `RegName` varchar(100) NOT NULL COMMENT '邀请的用户',
  `InviteIntegral` int(11) NOT NULL COMMENT '邀请获得的积分',
  `RegIntegral` int(11) NOT NULL COMMENT '被邀请获得的积分',
  `RegTime` datetime NOT NULL COMMENT '注册时间',
  `UserId` bigint(20) NOT NULL COMMENT '用户ID',
  `RegUserId` bigint(20) NOT NULL COMMENT '被邀请的用户ID',
  `RecordTime` datetime NOT NULL COMMENT '获得积分时间',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `InviteMember` (`UserId`) USING BTREE,
  KEY `RegMember` (`RegUserId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_inviterecord`
--

LOCK TABLES `mall_inviterecord` WRITE;
/*!40000 ALTER TABLE `mall_inviterecord` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_inviterecord` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_inviterule`
--

DROP TABLE IF EXISTS `mall_inviterule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_inviterule` (
  `Id` bigint(11) NOT NULL AUTO_INCREMENT,
  `InviteIntegral` int(11) NOT NULL COMMENT '邀请能获得的积分',
  `RegIntegral` int(11) NOT NULL COMMENT '被邀请能获得的积分',
  `ShareTitle` varchar(100) DEFAULT NULL COMMENT '分享标题',
  `ShareDesc` varchar(1000) DEFAULT NULL COMMENT '分享详细',
  `ShareIcon` varchar(200) DEFAULT NULL COMMENT '分享图标',
  `ShareRule` varchar(1000) DEFAULT NULL COMMENT '分享规则',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_inviterule`
--

LOCK TABLES `mall_inviterule` WRITE;
/*!40000 ALTER TABLE `mall_inviterule` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_inviterule` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_invoicecontext`
--

DROP TABLE IF EXISTS `mall_invoicecontext`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_invoicecontext` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL COMMENT '发票名称',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=51 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_invoicecontext`
--

LOCK TABLES `mall_invoicecontext` WRITE;
/*!40000 ALTER TABLE `mall_invoicecontext` DISABLE KEYS */;
INSERT INTO `mall_invoicecontext` VALUES (47,'个人'),(48,'公司');
/*!40000 ALTER TABLE `mall_invoicecontext` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_invoicetitle`
--

DROP TABLE IF EXISTS `mall_invoicetitle`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_invoicetitle` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `UserId` bigint(20) NOT NULL COMMENT '用户ID',
  `InvoiceType` int(11) NOT NULL DEFAULT '1' COMMENT '发票类型（1:普通发票、2:电子发票、3:增值税发票）',
  `Name` varchar(200) DEFAULT NULL COMMENT '抬头名称',
  `Code` varchar(200) DEFAULT NULL COMMENT '税号',
  `InvoiceContext` varchar(50) DEFAULT '0' COMMENT '发票明细',
  `RegisterAddress` varchar(200) DEFAULT NULL COMMENT '注册地址',
  `RegisterPhone` varchar(50) DEFAULT NULL COMMENT '注册电话',
  `BankName` varchar(100) DEFAULT NULL COMMENT '开户银行',
  `BankNo` varchar(50) DEFAULT NULL COMMENT '银行帐号',
  `RealName` varchar(50) DEFAULT NULL COMMENT '收票人姓名',
  `CellPhone` varchar(20) DEFAULT NULL COMMENT '收票人手机号',
  `Email` varchar(50) DEFAULT NULL COMMENT '收票人邮箱',
  `RegionID` int(11) NOT NULL DEFAULT '0' COMMENT '收票人地址区域ID',
  `Address` varchar(100) DEFAULT NULL COMMENT '收票人详细地址',
  `IsDefault` tinyint(4) NOT NULL COMMENT '是否默认',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=302 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_invoicetitle`
--

LOCK TABLES `mall_invoicetitle` WRITE;
/*!40000 ALTER TABLE `mall_invoicetitle` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_invoicetitle` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_label`
--

DROP TABLE IF EXISTS `mall_label`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_label` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `LabelName` varchar(50) NOT NULL COMMENT '标签名称',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=57 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_label`
--

LOCK TABLES `mall_label` WRITE;
/*!40000 ALTER TABLE `mall_label` DISABLE KEYS */;
INSERT INTO `mall_label` VALUES (33,'清雅'),(34,'购物狂'),(35,'常客');
/*!40000 ALTER TABLE `mall_label` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_log`
--

DROP TABLE IF EXISTS `mall_log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_log` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL,
  `PageUrl` varchar(1000) NOT NULL,
  `Date` datetime NOT NULL,
  `UserName` varchar(100) NOT NULL,
  `IPAddress` varchar(100) NOT NULL,
  `Description` varchar(1000) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=7448 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_log`
--

LOCK TABLES `mall_log` WRITE;
/*!40000 ALTER TABLE `mall_log` DISABLE KEYS */;
INSERT INTO `mall_log` VALUES (4608,0,'ProductType/SaveModel','2017-02-13 17:26:46','admin','120.26.125.164','修改平台类目,操作记录:id:0 '),(4609,0,'Category/Add','2017-02-13 17:27:03','admin','120.26.125.164','添加平台分类,操作记录:'),(4610,0,'Category/Add','2017-02-13 17:27:19','admin','120.26.125.164','添加平台分类,操作记录:'),(4611,0,'Category/Add','2017-02-13 17:27:28','admin','120.26.125.164','添加平台分类,操作记录:'),(4612,0,'Category/Add','2017-02-13 17:27:50','admin','120.26.125.164','添加平台分类,操作记录:'),(4613,0,'Category/Add','2017-02-13 17:28:03','admin','120.26.125.164','添加平台分类,操作记录:'),(4614,1,'/Product/Create','2017-02-13 17:32:49','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=三只松鼠_开口松子218gx2袋零食坚果特产炒货东北红松子原味 [成功]'),(4615,0,'Category/Add','2017-02-13 17:33:13','admin','120.26.125.164','添加平台分类,操作记录:'),(4616,0,'Category/Add','2017-02-13 17:33:22','admin','120.26.125.164','添加平台分类,操作记录:'),(4617,1,'/Product/Create','2017-02-13 17:36:12','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=三只松鼠_坚果组合613g零食坚果特产碧根果夏威夷果开口松子原味 [成功]'),(4618,1,'/Product/Create','2017-02-13 17:39:25','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=三只松鼠 坚果炒货 休闲零食 纸皮核桃210g/袋 [成功]'),(4619,1,'/Product/Create','2017-02-13 17:42:07','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=卫龙 休闲零食 辣条 小面筋 办公室休闲食品 22g*20包(新老包装随机发货) [成功]'),(4620,0,'ProductType/SaveModel','2017-02-13 17:47:20','admin','120.26.125.164','修改平台类目,操作记录:id:0 '),(4621,0,'Category/Add','2017-02-13 17:47:35','admin','120.26.125.164','添加平台分类,操作记录:'),(4622,0,'Category/Add','2017-02-13 17:47:44','admin','120.26.125.164','添加平台分类,操作记录:'),(4623,0,'Category/Add','2017-02-13 17:47:51','admin','120.26.125.164','添加平台分类,操作记录:'),(4624,0,'Category/Add','2017-02-13 17:48:01','admin','120.26.125.164','添加平台分类,操作记录:'),(4625,0,'Category/Add','2017-02-13 17:48:14','admin','120.26.125.164','添加平台分类,操作记录:'),(4626,1,'/Product/Create','2017-02-13 17:48:54','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=卫龙 休闲零食 亲嘴条 辣条 400g/袋 [成功]'),(4627,0,'Category/Add','2017-02-13 17:50:28','admin','120.26.125.164','添加平台分类,操作记录:'),(4628,1,'/Product/Create','2017-02-13 17:53:10','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=ONLY2017早春新品宽松猫咪图案假两件针织衫女L|117124507 G43花灰 170/88A/L [成功]'),(4629,1,'/Product/Create','2017-02-13 17:55:41','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=ONLY2017早春新品大V领宽松下摆开衩长袖针织衫女ES|117124502 F17庆典红 165/84A/M [成功]'),(4630,1,'/Product/Create','2017-02-13 17:58:45','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=ONLY2016早春新品拼色宽松太空棉卫衣女E|11639R511 07B酒红色 165/84A/M [成功]'),(4631,1,'/Product/Create','2017-02-13 18:01:31','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=ONLY春季新品含莱卡面料五分袖修身露肩一字领针织连衣裙女E|116361504 010黑 175/92A/XL [成功]'),(4632,1,'/Product/Create','2017-02-13 18:03:48','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=ONLY春季新品纯棉修身高腰镂空蕾丝连衣裙女L|116307501 021奶白 155/76A/XS [成功]'),(4633,1,'/Product/Create','2017-02-13 18:06:20','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=ONLY2016春季新品T恤衬衫叠搭纯棉两件套连衣裙女T|116360504 10D炭花灰 165/84A/M [成功]'),(4634,1,'category/CreateCategory','2017-02-13 18:06:38','selleradmin','120.26.125.164','创建店铺分类,操作记录:pid:0 name:衣服 '),(4635,1,'category/CreateCategory','2017-02-13 18:06:42','selleradmin','120.26.125.164','创建店铺分类,操作记录:pid:350 name:女装 '),(4636,1,'category/CreateCategory','2017-02-13 18:06:47','selleradmin','120.26.125.164','创建店铺分类,操作记录:pid:0 name:零食 '),(4637,1,'category/CreateCategory','2017-02-13 18:06:54','selleradmin','120.26.125.164','创建店铺分类,操作记录:pid:352 name:松子 '),(4638,1,'category/CreateCategory','2017-02-13 18:07:02','selleradmin','120.26.125.164','创建店铺分类,操作记录:pid:352 name:核桃 '),(4639,1,'/Product/Edit','2017-02-13 18:07:21','selleradmin','120.26.125.164','商家修改商品，Id=709, 名称=ONLY2016春季新品T恤衬衫叠搭纯棉两件套连衣裙女T|116360504 10D炭花灰 165/84A/M [成功]'),(4640,1,'/Product/Edit','2017-02-13 18:07:37','selleradmin','120.26.125.164','商家修改商品，Id=708, 名称=ONLY春季新品纯棉修身高腰镂空蕾丝连衣裙女L|116307501 021奶白 155/76A/XS [成功]'),(4641,1,'/Product/Edit','2017-02-13 18:07:51','selleradmin','120.26.125.164','商家修改商品，Id=707, 名称=ONLY春季新品含莱卡面料五分袖修身露肩一字领针织连衣裙女E|116361504 010黑 175/92A/XL [成功]'),(4642,1,'/Product/Edit','2017-02-13 18:08:11','selleradmin','120.26.125.164','商家修改商品，Id=703, 名称=卫龙 休闲零食 亲嘴条 辣条 400g/袋 [成功]'),(4643,1,'/Product/Edit','2017-02-13 18:08:28','selleradmin','120.26.125.164','商家修改商品，Id=701, 名称=三只松鼠 坚果炒货 休闲零食 纸皮核桃210g/袋 [成功]'),(4644,1,'/Product/Edit','2017-02-13 18:09:02','selleradmin','120.26.125.164','商家修改商品，Id=700, 名称=三只松鼠_坚果组合613g零食坚果特产碧根果夏威夷果开口松子原味 [成功]'),(4645,1,'/Product/Create','2017-02-13 18:11:32','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=ONLY2016早春新品纯棉宽松徽章贴布牛仔背带裙女T|116342524 390洗水牛仔蓝(928) 155/76A/XS [成功]'),(4646,0,'product/BatchAudit','2017-02-13 18:12:01','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4647,1,'/Product/Edit','2017-02-14 09:14:22','selleradmin','120.26.125.164','商家修改商品，Id=699, 名称=三只松鼠_开口松子218gx2袋零食坚果特产炒货东北红松子原味 [成功]'),(4648,1,'/Product/Edit','2017-02-14 09:14:53','selleradmin','120.26.125.164','商家修改商品，Id=700, 名称=三只松鼠_坚果组合613g零食坚果特产碧根果夏威夷果开口松子原味 [成功]'),(4649,0,'product/BatchAudit','2017-02-14 09:15:59','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4650,1,'/Product/Create','2017-02-14 09:31:16','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=FOREVER21 甜美花朵抽褶吊带连衣裙 黑色/铁锈色 S [成功]'),(4651,1,'/Product/Create','2017-02-14 09:35:31','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=[Forever21 Contemporary]纯色优雅及膝短袖连衣裙 香草色 S [成功]'),(4652,1,'/Product/Create','2017-02-14 09:39:10','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=XX FOREVER21 短款撞色条纹无袖针织衫 红色/白色 M [成功]'),(4653,0,'Category/Add','2017-02-14 09:39:56','admin','120.26.125.164','添加平台分类,操作记录:'),(4654,0,'Category/Add','2017-02-14 09:40:06','admin','120.26.125.164','添加平台分类,操作记录:'),(4655,0,'Category/Add','2017-02-14 09:40:20','admin','120.26.125.164','添加平台分类,操作记录:'),(4656,0,'ProductType/SaveModel','2017-02-14 10:07:41','admin','120.26.125.164','修改平台类目,操作记录:id:0 '),(4657,0,'Category/Edit','2017-02-14 10:07:49','admin','120.26.125.164','修改平台分类,操作记录:Id:14 '),(4658,0,'Category/Edit','2017-02-14 10:07:54','admin','120.26.125.164','修改平台分类,操作记录:Id:15 '),(4659,0,'Category/Edit','2017-02-14 10:08:00','admin','120.26.125.164','修改平台分类,操作记录:Id:16 '),(4660,1,'/Product/Create','2017-02-14 10:15:00','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=Apple MacBook Air 13.3英寸笔记本电脑 银色(Core i5 处理器/8GB内存/128GB SSD闪存 MMGF2CH/A) [成功]'),(4661,1,'/Product/Create','2017-02-14 10:19:20','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=Apple MacBook Pro 15.4英寸笔记本电脑 银色(Core i7 处理器/16GB内存/256GB SSD闪存/Retina屏 MJLQ2CH/A) [成功]'),(4662,0,'Category/Add','2017-02-14 10:20:54','admin','120.26.125.164','添加平台分类,操作记录:'),(4663,0,'Category/Add','2017-02-14 10:21:00','admin','120.26.125.164','添加平台分类,操作记录:'),(4664,0,'Category/Add','2017-02-14 10:21:06','admin','120.26.125.164','添加平台分类,操作记录:'),(4665,0,'Category/Add','2017-02-14 10:21:35','admin','120.26.125.164','添加平台分类,操作记录:'),(4666,0,'Category/Add','2017-02-14 10:21:42','admin','120.26.125.164','添加平台分类,操作记录:'),(4667,0,'Category/Add','2017-02-14 10:21:57','admin','120.26.125.164','添加平台分类,操作记录:'),(4668,0,'Category/Add','2017-02-14 10:22:06','admin','120.26.125.164','添加平台分类,操作记录:'),(4669,0,'ProductType/SaveModel','2017-02-14 10:28:51','admin','120.26.125.164','修改平台类目,操作记录:id:0 '),(4670,0,'Category/Edit','2017-02-14 10:29:09','admin','120.26.125.164','修改平台分类,操作记录:Id:17 '),(4671,0,'Category/Edit','2017-02-14 10:29:13','admin','120.26.125.164','修改平台分类,操作记录:Id:18 '),(4672,0,'Category/Edit','2017-02-14 10:29:17','admin','120.26.125.164','修改平台分类,操作记录:Id:19 '),(4673,1,'/Product/Create','2017-02-14 10:31:34','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=宇食俱进厄瓜多尔白虾大虾南美活冻海鲜水产海虾 1700克40-50白虾 [成功]'),(4674,0,'ProductType/SaveModel','2017-02-14 10:33:27','admin','120.26.125.164','修改平台类目,操作记录:id:0 '),(4675,0,'Brand/Edit','2017-02-14 10:36:20','admin','120.26.125.164','编辑品牌,操作记录:'),(4676,0,'ProductType/SaveModel','2017-02-14 10:38:00','admin','120.26.125.164','修改平台类目,操作记录:id:0 '),(4677,0,'Category/Edit','2017-02-14 10:38:08','admin','120.26.125.164','修改平台分类,操作记录:Id:20 '),(4678,0,'Category/Edit','2017-02-14 10:38:17','admin','120.26.125.164','修改平台分类,操作记录:Id:21 '),(4679,0,'Category/Edit','2017-02-14 10:38:22','admin','120.26.125.164','修改平台分类,操作记录:Id:22 '),(4680,0,'Category/Edit','2017-02-14 10:38:26','admin','120.26.125.164','修改平台分类,操作记录:Id:23 '),(4681,0,'Brand/Edit','2017-02-14 10:40:34','admin','120.26.125.164','编辑品牌,操作记录:'),(4682,0,'Brand/Edit','2017-02-14 10:41:22','admin','120.26.125.164','编辑品牌,操作记录:'),(4683,1,'/Product/Create','2017-02-14 10:42:14','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=欧莱雅水能润泽双效洁面膏100+50ml [成功]'),(4684,0,'ProductType/SaveModel','2017-02-14 10:43:55','admin','120.26.125.164','修改平台类目,操作记录:id:87 '),(4685,0,'Gift/Edit','2017-02-14 10:44:15','admin','120.26.125.164','操作礼品信息,操作记录:'),(4686,0,'ProductType/SaveModel','2017-02-14 10:45:36','admin','120.26.125.164','修改平台类目,操作记录:id:84 '),(4687,0,'ProductType/SaveModel','2017-02-14 10:46:14','admin','120.26.125.164','修改平台类目,操作记录:id:0 '),(4688,0,'ProductType/SaveModel','2017-02-14 10:46:57','admin','120.26.125.164','修改平台类目,操作记录:id:0 '),(4689,0,'ProductType/SaveModel','2017-02-14 10:47:10','admin','120.26.125.164','修改平台类目,操作记录:id:0 '),(4690,0,'ProductType/SaveModel','2017-02-14 10:47:23','admin','120.26.125.164','修改平台类目,操作记录:id:0 '),(4691,1,'/Product/Create','2017-02-14 10:47:39','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=贝德玛（Bioderma）深层舒妍卸妆水 舒缓保湿粉水（干性中性敏感肌法国版/海外版随机发）500ml  [成功]'),(4692,0,'ProductType/SaveModel','2017-02-14 10:47:44','admin','120.26.125.164','修改平台类目,操作记录:id:0 '),(4693,0,'ProductType/SaveModel','2017-02-14 10:47:57','admin','120.26.125.164','修改平台类目,操作记录:id:0 '),(4694,0,'ProductType/SaveModel','2017-02-14 10:48:09','admin','120.26.125.164','修改平台类目,操作记录:id:0 '),(4695,0,'product/BatchAudit','2017-02-14 10:48:17','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4696,1,'category/CreateCategory','2017-02-14 10:48:47','selleradmin','120.26.125.164','创建店铺分类,操作记录:pid:0 name:生鲜 '),(4697,1,'category/CreateCategory','2017-02-14 10:48:54','selleradmin','120.26.125.164','创建店铺分类,操作记录:pid:355 name:虾类 '),(4698,1,'category/CreateCategory','2017-02-14 10:49:00','selleradmin','120.26.125.164','创建店铺分类,操作记录:pid:0 name:电脑 '),(4699,1,'category/CreateCategory','2017-02-14 10:49:05','selleradmin','120.26.125.164','创建店铺分类,操作记录:pid:357 name:笔记本 '),(4700,1,'category/CreateCategory','2017-02-14 10:49:29','selleradmin','120.26.125.164','创建店铺分类,操作记录:pid:0 name:个护 '),(4701,1,'category/CreateCategory','2017-02-14 10:49:36','selleradmin','120.26.125.164','创建店铺分类,操作记录:pid:359 name:洁面 '),(4702,1,'category/UpdateName','2017-02-14 10:50:31','selleradmin','120.26.125.164','修改店铺分类名称,操作记录:id:360 name:洁面/卸妆 '),(4703,1,'/Product/Edit','2017-02-14 10:50:43','selleradmin','120.26.125.164','商家修改商品，Id=718, 名称=贝德玛（Bioderma）深层舒妍卸妆水 舒缓保湿粉水（干性中性敏感肌法国版/海外版随机发）500ml  [成功]'),(4704,1,'/Product/Edit','2017-02-14 10:50:55','selleradmin','120.26.125.164','商家修改商品，Id=717, 名称=欧莱雅水能润泽双效洁面膏100+50ml [成功]'),(4705,1,'/Product/Edit','2017-02-14 10:51:11','selleradmin','120.26.125.164','商家修改商品，Id=716, 名称=宇食俱进厄瓜多尔白虾大虾南美活冻海鲜水产海虾 1700克40-50白虾 [成功]'),(4706,1,'/Product/Edit','2017-02-14 10:51:21','selleradmin','120.26.125.164','商家修改商品，Id=715, 名称=Apple MacBook Pro 15.4英寸笔记本电脑 银色(Core i7 处理器/16GB内存/256GB SSD闪存/Retina屏 MJLQ2CH/A) [成功]'),(4707,1,'/Product/Edit','2017-02-14 10:51:32','selleradmin','120.26.125.164','商家修改商品，Id=714, 名称=Apple MacBook Air 13.3英寸笔记本电脑 银色(Core i5 处理器/8GB内存/128GB SSD闪存 MMGF2CH/A) [成功]'),(4708,1,'/Product/Edit','2017-02-14 10:51:44','selleradmin','120.26.125.164','商家修改商品，Id=713, 名称=XX FOREVER21 短款撞色条纹无袖针织衫 红色/白色 M [成功]'),(4709,1,'/Product/Edit','2017-02-14 10:52:03','selleradmin','120.26.125.164','商家修改商品，Id=708, 名称=ONLY春季新品纯棉修身高腰镂空蕾丝连衣裙女L|116307501 021奶白 155/76A/XS [成功]'),(4710,0,'product/BatchAudit','2017-02-14 10:52:17','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4711,0,'Brand/Edit','2017-02-14 11:00:24','admin','120.26.125.164','编辑品牌,操作记录:'),(4712,0,'ProductType/SaveModel','2017-02-14 11:05:52','admin','120.26.125.164','修改平台类目,操作记录:id:93 '),(4713,0,'Category/Add','2017-02-14 11:09:44','admin','120.26.125.164','添加平台分类,操作记录:'),(4714,0,'Category/Add','2017-02-14 11:11:07','admin','120.26.125.164','添加平台分类,操作记录:'),(4715,0,'Category/Add','2017-02-14 11:11:29','admin','120.26.125.164','添加平台分类,操作记录:'),(4716,0,'Category/Add','2017-02-14 11:12:03','admin','120.26.125.164','添加平台分类,操作记录:'),(4717,0,'Category/DeleteCategoryById','2017-02-14 11:12:16','admin','120.26.125.164','删除平台分类,操作记录:Id:27 '),(4718,0,'Category/Add','2017-02-14 11:12:26','admin','120.26.125.164','添加平台分类,操作记录:'),(4719,0,'Category/Add','2017-02-14 11:12:45','admin','120.26.125.164','添加平台分类,操作记录:'),(4720,0,'Category/Add','2017-02-14 11:12:53','admin','120.26.125.164','添加平台分类,操作记录:'),(4721,0,'Category/Add','2017-02-14 11:13:14','admin','120.26.125.164','添加平台分类,操作记录:'),(4722,0,'Category/Add','2017-02-14 11:13:38','admin','120.26.125.164','添加平台分类,操作记录:'),(4723,0,'Category/Add','2017-02-14 11:13:54','admin','120.26.125.164','添加平台分类,操作记录:'),(4724,0,'Category/Add','2017-02-14 11:14:07','admin','120.26.125.164','添加平台分类,操作记录:'),(4725,0,'Category/Add','2017-02-14 11:14:29','admin','120.26.125.164','添加平台分类,操作记录:'),(4726,0,'Category/Add','2017-02-14 11:14:51','admin','120.26.125.164','添加平台分类,操作记录:'),(4727,0,'Category/Add','2017-02-14 11:16:15','admin','120.26.125.164','添加平台分类,操作记录:'),(4728,0,'Category/Add','2017-02-14 11:16:40','admin','120.26.125.164','添加平台分类,操作记录:'),(4729,0,'Category/Edit','2017-02-14 11:18:21','admin','120.26.125.164','修改平台分类,操作记录:Id:8 '),(4730,0,'Category/Add','2017-02-14 11:18:54','admin','120.26.125.164','添加平台分类,操作记录:'),(4731,0,'Category/Add','2017-02-14 11:19:14','admin','120.26.125.164','添加平台分类,操作记录:'),(4732,0,'Category/Add','2017-02-14 11:19:29','admin','120.26.125.164','添加平台分类,操作记录:'),(4733,0,'Category/Add','2017-02-14 11:19:45','admin','120.26.125.164','添加平台分类,操作记录:'),(4734,0,'Category/Add','2017-02-14 11:20:05','admin','120.26.125.164','添加平台分类,操作记录:'),(4735,1,'/Product/BatchSaleOff','2017-02-14 11:27:34','selleradmin','120.26.125.164','商家商品批量下架，Ids=710,711,712'),(4736,1,'/Product/BatchSaleOff','2017-02-14 11:27:46','selleradmin','120.26.125.164','商家商品批量下架，Ids=706'),(4737,1,'/Product/BatchSaleOff','2017-02-14 11:27:57','selleradmin','120.26.125.164','商家商品批量下架，Ids=715'),(4738,1,'/Product/BatchOnSale','2017-02-14 11:30:45','selleradmin','120.26.125.164','商家商品批量上架，Ids=715,712,711,710,706'),(4739,0,'LimitTimeBuy/AuditItem','2017-02-14 11:45:50','admin','120.26.125.164','审核商品状态,操作记录:'),(4740,0,'LimitTimeBuy/AuditItem','2017-02-14 11:45:55','admin','120.26.125.164','审核商品状态,操作记录:'),(4741,0,'LimitTimeBuy/AuditItem','2017-02-14 11:45:59','admin','120.26.125.164','审核商品状态,操作记录:'),(4742,0,'LimitTimeBuy/AuditItem','2017-02-14 11:46:03','admin','120.26.125.164','审核商品状态,操作记录:'),(4743,0,'LimitTimeBuy/AuditItem','2017-02-14 11:46:07','admin','120.26.125.164','审核商品状态,操作记录:'),(4744,0,'Category/Edit','2017-02-14 14:06:39','admin','120.26.125.164','修改平台分类,操作记录:Id:1 '),(4745,0,'Category/Add','2017-02-14 14:08:25','admin','120.26.125.164','添加平台分类,操作记录:'),(4746,0,'Category/Edit','2017-02-14 14:08:45','admin','120.26.125.164','修改平台分类,操作记录:Id:20 '),(4747,0,'Category/Edit','2017-02-14 14:09:00','admin','120.26.125.164','修改平台分类,操作记录:Id:41 '),(4748,1,'/Product/Create','2017-02-14 14:10:56','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=资生堂洗颜专科柔澈泡沫洁面乳１２０ｇ [成功]'),(4749,0,'Category/Add','2017-02-14 14:11:42','admin','120.26.125.164','添加平台分类,操作记录:'),(4750,1,'/Product/Create','2017-02-14 14:13:39','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=欧莱雅（LOREAL）男士火山岩控油清痘洁面膏100ml [成功]'),(4751,0,'Category/Add','2017-02-14 14:13:52','admin','120.26.125.164','添加平台分类,操作记录:'),(4752,0,'Category/Add','2017-02-14 14:14:05','admin','120.26.125.164','添加平台分类,操作记录:'),(4753,0,'Category/Add','2017-02-14 14:14:13','admin','120.26.125.164','添加平台分类,操作记录:'),(4754,0,'Category/Add','2017-02-14 14:14:35','admin','120.26.125.164','添加平台分类,操作记录:'),(4755,0,'Category/Add','2017-02-14 14:14:54','admin','120.26.125.164','添加平台分类,操作记录:'),(4756,0,'Category/Add','2017-02-14 14:15:12','admin','120.26.125.164','添加平台分类,操作记录:'),(4757,0,'Category/Add','2017-02-14 14:15:21','admin','120.26.125.164','添加平台分类,操作记录:'),(4758,0,'Category/Add','2017-02-14 14:15:30','admin','120.26.125.164','添加平台分类,操作记录:'),(4759,0,'Category/Add','2017-02-14 14:15:52','admin','120.26.125.164','添加平台分类,操作记录:'),(4760,0,'Category/Add','2017-02-14 14:16:00','admin','120.26.125.164','添加平台分类,操作记录:'),(4761,0,'Category/Add','2017-02-14 14:16:17','admin','120.26.125.164','添加平台分类,操作记录:'),(4762,0,'Category/Add','2017-02-14 14:16:31','admin','120.26.125.164','添加平台分类,操作记录:'),(4763,1,'/Product/Create','2017-02-14 14:16:32','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=丝塔芙(Cetaphil)洁面乳118ml [成功]'),(4764,0,'Category/Add','2017-02-14 14:16:47','admin','120.26.125.164','添加平台分类,操作记录:'),(4765,0,'Category/Add','2017-02-14 14:17:12','admin','120.26.125.164','添加平台分类,操作记录:'),(4766,0,'Category/Add','2017-02-14 14:17:31','admin','120.26.125.164','添加平台分类,操作记录:'),(4767,0,'Category/Add','2017-02-14 14:17:47','admin','120.26.125.164','添加平台分类,操作记录:'),(4768,0,'Category/Add','2017-02-14 14:17:58','admin','120.26.125.164','添加平台分类,操作记录:'),(4769,0,'Category/Add','2017-02-14 14:18:56','admin','120.26.125.164','添加平台分类,操作记录:'),(4770,1,'/Product/Create','2017-02-14 14:19:02','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=Clinique倩碧液体洁面皂温和型200ml [成功]'),(4771,0,'Category/Add','2017-02-14 14:19:34','admin','120.26.125.164','添加平台分类,操作记录:'),(4772,1,'/Product/Create','2017-02-14 14:21:01','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=露得清深层净化洗面乳2支装100g*2 [成功]'),(4773,1,'/Product/BatchSaleOff','2017-02-14 14:21:45','selleradmin','120.26.125.164','商家商品批量下架，Ids=718,715,712,711'),(4774,1,'/Product/BatchSaleOff','2017-02-14 14:22:00','selleradmin','120.26.125.164','商家商品批量下架，Ids=703'),(4775,0,'Category/Add','2017-02-14 14:23:19','admin','120.26.125.164','添加平台分类,操作记录:'),(4776,0,'Category/Add','2017-02-14 14:23:34','admin','120.26.125.164','添加平台分类,操作记录:'),(4777,0,'Category/Add','2017-02-14 14:23:53','admin','120.26.125.164','添加平台分类,操作记录:'),(4778,0,'Category/Add','2017-02-14 14:24:32','admin','120.26.125.164','添加平台分类,操作记录:'),(4779,0,'Category/Add','2017-02-14 14:24:40','admin','120.26.125.164','添加平台分类,操作记录:'),(4780,0,'Category/Add','2017-02-14 14:24:52','admin','120.26.125.164','添加平台分类,操作记录:'),(4781,0,'Category/Add','2017-02-14 14:25:09','admin','120.26.125.164','添加平台分类,操作记录:'),(4782,0,'Category/Add','2017-02-14 14:25:27','admin','120.26.125.164','添加平台分类,操作记录:'),(4783,0,'Category/Add','2017-02-14 14:25:35','admin','120.26.125.164','添加平台分类,操作记录:'),(4784,0,'Category/Add','2017-02-14 14:25:51','admin','120.26.125.164','添加平台分类,操作记录:'),(4785,1,'/Product/Create','2017-02-14 14:26:18','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=2016秋冬新款女装中长款羊毛大衣修身显瘦风衣西装格子毛呢外套女 [成功]'),(4786,0,'Category/Add','2017-02-14 14:26:29','admin','120.26.125.164','添加平台分类,操作记录:'),(4787,1,'/Product/Edit','2017-02-14 14:26:31','selleradmin','120.26.125.164','商家修改商品，Id=724, 名称=2016秋冬新款女装中长款羊毛大衣修身显瘦风衣西装格子毛呢外套女 [成功]'),(4788,0,'Category/Add','2017-02-14 14:26:40','admin','120.26.125.164','添加平台分类,操作记录:'),(4789,0,'Category/Add','2017-02-14 14:26:48','admin','120.26.125.164','添加平台分类,操作记录:'),(4790,0,'product/BatchAudit','2017-02-14 14:27:02','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4791,0,'Category/Add','2017-02-14 14:27:05','admin','120.26.125.164','添加平台分类,操作记录:'),(4792,1,'/Product/Delete','2017-02-14 14:27:19','selleradmin','120.26.125.164','商家删除商品，Ids=724'),(4793,0,'Category/Add','2017-02-14 14:27:22','admin','120.26.125.164','添加平台分类,操作记录:'),(4794,0,'Category/Add','2017-02-14 14:27:32','admin','120.26.125.164','添加平台分类,操作记录:'),(4795,0,'Category/Add','2017-02-14 14:27:46','admin','120.26.125.164','添加平台分类,操作记录:'),(4796,0,'Category/Add','2017-02-14 14:27:52','admin','120.26.125.164','添加平台分类,操作记录:'),(4797,0,'Category/Add','2017-02-14 14:28:04','admin','120.26.125.164','添加平台分类,操作记录:'),(4798,0,'Category/Add','2017-02-14 14:28:11','admin','120.26.125.164','添加平台分类,操作记录:'),(4799,0,'Category/Add','2017-02-14 14:28:18','admin','120.26.125.164','添加平台分类,操作记录:'),(4800,0,'Category/Add','2017-02-14 14:28:31','admin','120.26.125.164','添加平台分类,操作记录:'),(4801,1,'/Product/Create','2017-02-14 14:28:50','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=2016秋冬新款女装中长款羊毛大衣修身显瘦风衣西装格子毛呢外套女 [成功]'),(4802,0,'Category/Add','2017-02-14 14:28:51','admin','120.26.125.164','添加平台分类,操作记录:'),(4803,0,'Category/Add','2017-02-14 14:29:07','admin','120.26.125.164','添加平台分类,操作记录:'),(4804,0,'Category/Add','2017-02-14 14:29:26','admin','120.26.125.164','添加平台分类,操作记录:'),(4805,0,'Category/Add','2017-02-14 14:29:35','admin','120.26.125.164','添加平台分类,操作记录:'),(4806,0,'Category/Add','2017-02-14 14:29:54','admin','120.26.125.164','添加平台分类,操作记录:'),(4807,0,'Category/Add','2017-02-14 14:30:02','admin','120.26.125.164','添加平台分类,操作记录:'),(4808,0,'Category/Add','2017-02-14 14:30:19','admin','120.26.125.164','添加平台分类,操作记录:'),(4809,0,'Category/Add','2017-02-14 14:30:27','admin','120.26.125.164','添加平台分类,操作记录:'),(4810,0,'Category/Add','2017-02-14 14:30:33','admin','120.26.125.164','添加平台分类,操作记录:'),(4811,0,'Category/Add','2017-02-14 14:30:43','admin','120.26.125.164','添加平台分类,操作记录:'),(4812,0,'Category/Add','2017-02-14 14:30:59','admin','120.26.125.164','添加平台分类,操作记录:'),(4813,0,'Category/Add','2017-02-14 14:31:25','admin','120.26.125.164','添加平台分类,操作记录:'),(4814,0,'Category/Add','2017-02-14 14:31:32','admin','120.26.125.164','添加平台分类,操作记录:'),(4815,0,'Category/Add','2017-02-14 14:31:40','admin','120.26.125.164','添加平台分类,操作记录:'),(4816,0,'Category/Add','2017-02-14 14:31:47','admin','120.26.125.164','添加平台分类,操作记录:'),(4817,0,'Category/Edit','2017-02-14 14:31:55','admin','120.26.125.164','修改平台分类,操作记录:Id:101 '),(4818,0,'Category/Add','2017-02-14 14:32:15','admin','120.26.125.164','添加平台分类,操作记录:'),(4819,0,'Category/Add','2017-02-14 14:32:24','admin','120.26.125.164','添加平台分类,操作记录:'),(4820,0,'Category/Add','2017-02-14 14:32:32','admin','120.26.125.164','添加平台分类,操作记录:'),(4821,1,'/Product/Create','2017-02-14 14:32:43','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=佐露丝女装2017新款韩版麂皮毛毛绒外套女中长款秋冬保暖加绒大衣宽松风衣潮 浅灰 S [成功]'),(4822,0,'Category/Add','2017-02-14 14:32:54','admin','120.26.125.164','添加平台分类,操作记录:'),(4823,0,'Category/Add','2017-02-14 14:33:09','admin','120.26.125.164','添加平台分类,操作记录:'),(4824,0,'Category/Add','2017-02-14 14:33:20','admin','120.26.125.164','添加平台分类,操作记录:'),(4825,0,'Category/Add','2017-02-14 14:33:28','admin','120.26.125.164','添加平台分类,操作记录:'),(4826,0,'Category/Add','2017-02-14 14:33:34','admin','120.26.125.164','添加平台分类,操作记录:'),(4827,0,'Category/Add','2017-02-14 14:33:43','admin','120.26.125.164','添加平台分类,操作记录:'),(4828,0,'Category/Add','2017-02-14 14:33:51','admin','120.26.125.164','添加平台分类,操作记录:'),(4829,0,'Category/Add','2017-02-14 14:34:15','admin','120.26.125.164','添加平台分类,操作记录:'),(4830,0,'Category/Add','2017-02-14 14:34:22','admin','120.26.125.164','添加平台分类,操作记录:'),(4831,0,'Category/Add','2017-02-14 14:34:40','admin','120.26.125.164','添加平台分类,操作记录:'),(4832,0,'Category/Add','2017-02-14 14:34:48','admin','120.26.125.164','添加平台分类,操作记录:'),(4833,1,'/Product/Create','2017-02-14 14:34:50','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=秀族 2016秋装新款韩版毛呢外套女装宽松粉色大衣中长款 [成功]'),(4834,0,'Category/Add','2017-02-14 14:35:10','admin','120.26.125.164','添加平台分类,操作记录:'),(4835,0,'Category/Add','2017-02-14 14:35:27','admin','120.26.125.164','添加平台分类,操作记录:'),(4836,0,'Category/Add','2017-02-14 14:35:34','admin','120.26.125.164','添加平台分类,操作记录:'),(4837,0,'Category/Add','2017-02-14 14:35:42','admin','120.26.125.164','添加平台分类,操作记录:'),(4838,0,'Category/Add','2017-02-14 14:35:48','admin','120.26.125.164','添加平台分类,操作记录:'),(4839,0,'Category/Add','2017-02-14 14:36:02','admin','120.26.125.164','添加平台分类,操作记录:'),(4840,0,'Category/Add','2017-02-14 14:36:39','admin','120.26.125.164','添加平台分类,操作记录:'),(4841,0,'Category/Add','2017-02-14 14:36:47','admin','120.26.125.164','添加平台分类,操作记录:'),(4842,0,'Category/Add','2017-02-14 14:37:03','admin','120.26.125.164','添加平台分类,操作记录:'),(4843,1,'/Product/Create','2017-02-14 14:37:13','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=米兰茵韩版茧型加棉羊毛呢子外套中长款宽松加厚羊绒大衣女装 [成功]'),(4844,0,'Category/Add','2017-02-14 14:37:30','admin','120.26.125.164','添加平台分类,操作记录:'),(4845,0,'Category/Add','2017-02-14 14:37:45','admin','120.26.125.164','添加平台分类,操作记录:'),(4846,0,'Category/Add','2017-02-14 14:37:55','admin','120.26.125.164','添加平台分类,操作记录:'),(4847,0,'Category/Add','2017-02-14 14:38:18','admin','120.26.125.164','添加平台分类,操作记录:'),(4848,0,'Category/Add','2017-02-14 14:38:45','admin','120.26.125.164','添加平台分类,操作记录:'),(4849,0,'Category/Add','2017-02-14 14:39:05','admin','120.26.125.164','添加平台分类,操作记录:'),(4850,0,'Category/Add','2017-02-14 14:39:14','admin','120.26.125.164','添加平台分类,操作记录:'),(4851,0,'Category/Add','2017-02-14 14:42:10','admin','120.26.125.164','添加平台分类,操作记录:'),(4852,0,'Category/Add','2017-02-14 14:42:24','admin','120.26.125.164','添加平台分类,操作记录:'),(4853,1,'/Product/Create','2017-02-14 14:42:57','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=2017秋冬加厚韩国韩范宽松中长款毛呢外套显瘦纯色大翻领 [成功]'),(4854,0,'Category/Add','2017-02-14 14:43:19','admin','120.26.125.164','添加平台分类,操作记录:'),(4855,0,'Category/Add','2017-02-14 14:43:28','admin','120.26.125.164','添加平台分类,操作记录:'),(4856,1,'/Product/Create','2017-02-14 14:44:05','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=2017秋冬加厚韩国韩范宽松中长款毛呢外套显瘦纯色大翻领 [成功]'),(4857,0,'Category/Add','2017-02-14 14:44:19','admin','120.26.125.164','添加平台分类,操作记录:'),(4858,0,'Category/Add','2017-02-14 14:44:26','admin','120.26.125.164','添加平台分类,操作记录:'),(4859,1,'/Product/Delete','2017-02-14 14:44:29','selleradmin','120.26.125.164','商家删除商品，Ids=729'),(4860,0,'Category/Add','2017-02-14 14:44:38','admin','120.26.125.164','添加平台分类,操作记录:'),(4861,0,'Category/Add','2017-02-14 14:44:45','admin','120.26.125.164','添加平台分类,操作记录:'),(4862,0,'Category/Add','2017-02-14 14:44:51','admin','120.26.125.164','添加平台分类,操作记录:'),(4863,0,'Category/Add','2017-02-14 14:45:24','admin','120.26.125.164','添加平台分类,操作记录:'),(4864,0,'Category/Add','2017-02-14 14:45:41','admin','120.26.125.164','添加平台分类,操作记录:'),(4865,0,'Category/Add','2017-02-14 14:46:13','admin','120.26.125.164','添加平台分类,操作记录:'),(4866,0,'Category/Add','2017-02-14 14:46:21','admin','120.26.125.164','添加平台分类,操作记录:'),(4867,0,'Category/Add','2017-02-14 14:46:41','admin','120.26.125.164','添加平台分类,操作记录:'),(4868,0,'Category/Add','2017-02-14 14:46:51','admin','120.26.125.164','添加平台分类,操作记录:'),(4869,0,'Category/Add','2017-02-14 14:46:58','admin','120.26.125.164','添加平台分类,操作记录:'),(4870,0,'Category/Add','2017-02-14 14:47:05','admin','120.26.125.164','添加平台分类,操作记录:'),(4871,0,'Category/Add','2017-02-14 14:47:20','admin','120.26.125.164','添加平台分类,操作记录:'),(4872,0,'Category/Add','2017-02-14 14:47:28','admin','120.26.125.164','添加平台分类,操作记录:'),(4873,0,'product/BatchAudit','2017-02-14 14:48:58','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4874,0,'ProductType/SaveModel','2017-02-14 14:49:41','admin','120.26.125.164','修改平台类目,操作记录:id:0 '),(4875,0,'ProductType/SaveModel','2017-02-14 14:49:48','admin','120.26.125.164','修改平台类目,操作记录:id:0 '),(4876,0,'Category/Add','2017-02-14 14:50:07','admin','120.26.125.164','添加平台分类,操作记录:'),(4877,0,'Category/Add','2017-02-14 14:50:15','admin','120.26.125.164','添加平台分类,操作记录:'),(4878,0,'Category/Edit','2017-02-14 14:57:48','admin','120.26.125.164','修改平台分类,操作记录:Id:8 '),(4879,0,'Category/Edit','2017-02-14 14:58:06','admin','120.26.125.164','修改平台分类,操作记录:Id:1 '),(4880,0,'Category/Edit','2017-02-14 14:58:35','admin','120.26.125.164','修改平台分类,操作记录:Id:20 '),(4881,0,'Category/Edit','2017-02-14 14:59:05','admin','120.26.125.164','修改平台分类,操作记录:Id:39 '),(4882,0,'Category/Edit','2017-02-14 14:59:26','admin','120.26.125.164','修改平台分类,操作记录:Id:40 '),(4883,0,'Category/Edit','2017-02-14 14:59:34','admin','120.26.125.164','修改平台分类,操作记录:Id:41 '),(4884,0,'Category/Edit','2017-02-14 14:59:50','admin','120.26.125.164','修改平台分类,操作记录:Id:42 '),(4885,0,'Category/Edit','2017-02-14 14:59:59','admin','120.26.125.164','修改平台分类,操作记录:Id:44 '),(4886,0,'Category/Edit','2017-02-14 15:00:09','admin','120.26.125.164','修改平台分类,操作记录:Id:43 '),(4887,0,'Category/Edit','2017-02-14 15:00:27','admin','120.26.125.164','修改平台分类,操作记录:Id:45 '),(4888,0,'Category/Edit','2017-02-14 15:00:39','admin','120.26.125.164','修改平台分类,操作记录:Id:151 '),(4889,0,'Category/Edit','2017-02-14 15:00:45','admin','120.26.125.164','修改平台分类,操作记录:Id:152 '),(4890,0,'Category/Edit','2017-02-14 15:01:09','admin','120.26.125.164','修改平台分类,操作记录:Id:24 '),(4891,0,'Category/Edit','2017-02-14 15:01:27','admin','120.26.125.164','修改平台分类,操作记录:Id:17 '),(4892,0,'Category/Edit','2017-02-14 15:01:37','admin','120.26.125.164','修改平台分类,操作记录:Id:25 '),(4893,0,'Category/Edit','2017-02-14 15:01:56','admin','120.26.125.164','修改平台分类,操作记录:Id:14 '),(4894,0,'LimitTimeBuy/AuditItem','2017-02-14 15:09:29','admin','120.26.125.164','审核商品状态,操作记录:'),(4895,1,'/Product/Create','2017-02-14 15:32:56','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=蔓斯菲尔 设计师椅 简约时尚休闲塑料椅 创意电脑椅子 办公餐椅 会议椅 [成功]'),(4896,0,'product/BatchAudit','2017-02-14 15:33:10','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4897,1,'/Product/Create','2017-02-14 15:37:47','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=全实木电脑桌书桌办公桌 [成功]'),(4898,0,'product/BatchAudit','2017-02-14 15:37:54','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4899,1,'/Product/Create','2017-02-14 15:44:53','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=丹麦依诺维绅 功能沙发床 时尚沙发 书房必用 小鸟 [成功]'),(4900,0,'product/BatchAudit','2017-02-14 15:45:18','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4901,0,'LimitTimeBuy/AuditItem','2017-02-14 15:53:12','admin','120.26.125.164','审核商品状态,操作记录:'),(4902,1,'/Product/Create','2017-02-14 15:54:22','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=原色木质简约现代艺术 家居配饰 软装配饰 可调光 木制DIY蘑菇台灯 [成功]'),(4903,0,'product/BatchAudit','2017-02-14 15:54:30','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4904,1,'/Product/Create','2017-02-14 15:59:54','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=惠宝隆无铅水晶红酒醒酒器蜗牛形干红葡萄酒醒酒壶分酒器500ml [成功]'),(4905,0,'product/BatchAudit','2017-02-14 16:00:01','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4906,0,'LimitTimeBuy/AuditItem','2017-02-14 16:02:08','admin','120.26.125.164','审核商品状态,操作记录:'),(4907,0,'LimitTimeBuy/AuditItem','2017-02-14 16:02:13','admin','120.26.125.164','审核商品状态,操作记录:'),(4908,0,'LimitTimeBuy/AuditItem','2017-02-14 16:03:53','admin','120.26.125.164','审核商品状态,操作记录:'),(4909,0,'LimitTimeBuy/AuditItem','2017-02-14 16:04:55','admin','120.26.125.164','审核商品状态,操作记录:'),(4910,0,'LimitTimeBuy/AuditItem','2017-02-14 16:06:05','admin','120.26.125.164','审核商品状态,操作记录:'),(4911,0,'LimitTimeBuy/AuditItem','2017-02-14 16:06:46','admin','120.26.125.164','审核商品状态,操作记录:'),(4912,0,'LimitTimeBuy/AuditItem','2017-02-14 16:07:54','admin','120.26.125.164','审核商品状态,操作记录:'),(4913,0,'LimitTimeBuy/AuditItem','2017-02-14 16:08:04','admin','120.26.125.164','审核商品状态,操作记录:'),(4914,1,'/Product/Create','2017-02-14 16:11:42','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=妮维雅男士水活多效洁面乳100g [成功]'),(4915,1,'/Product/Create','2017-02-14 16:13:14','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=怡鲜来 法国新鲜冷冻银鳕鱼中段 200g 进口海鲜水产 深海野生鳕鱼 [成功]'),(4916,1,'/Product/Create','2017-02-14 16:14:19','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=妮维雅男士水活畅透精华洁面液150ml [成功]'),(4917,0,'Category/Add','2017-02-14 16:15:28','admin','120.26.125.164','添加平台分类,操作记录:'),(4918,0,'Category/Add','2017-02-14 16:16:00','admin','120.26.125.164','添加平台分类,操作记录:'),(4919,1,'/Product/Create','2017-02-14 16:17:33','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=联想（Lenovo）Y50p-70 15.6英寸游戏笔记本电脑（I5-4210H 1T GTX960M升级版 ） [成功]'),(4920,1,'/Product/Create','2017-02-14 16:18:19','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=红霞草莓 3斤 单果20-30克 新鲜水果 SG [成功]'),(4921,1,'/Product/Create','2017-02-14 16:20:06','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=华硕（ASUS）FX50VX 15.6英寸游戏本（I7-6700 8G 1T GTX950M 2GB Win10 黑红） [成功]'),(4922,1,'/Product/Edit','2017-02-14 16:20:26','selleradmin','120.26.125.164','商家修改商品，Id=736, 名称=妮维雅男士水活多效洁面乳100g [成功]'),(4923,0,'product/BatchAudit','2017-02-14 16:20:36','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4924,1,'/Product/Create','2017-02-14 16:21:09','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=怡鲜来 阿根廷红虾2000g 4斤/盒 大号L2级 40-60尾 [成功]'),(4925,0,'LimitTimeBuy/AuditItem','2017-02-14 16:21:29','admin','120.26.125.164','审核商品状态,操作记录:'),(4926,0,'LimitTimeBuy/AuditItem','2017-02-14 16:21:33','admin','120.26.125.164','审核商品状态,操作记录:'),(4927,0,'Category/Add','2017-02-14 16:24:24','admin','120.26.125.164','添加平台分类,操作记录:'),(4928,0,'Category/Add','2017-02-14 16:24:54','admin','120.26.125.164','添加平台分类,操作记录:'),(4929,1,'/Product/Create','2017-02-14 16:27:55','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=勇艺达小勇机器人Y50B 太空银 家庭陪伴 启智教育 声控智能家居 视频监控 [成功]'),(4930,0,'product/BatchAudit','2017-02-14 16:34:01','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4931,0,'product/BatchAudit','2017-02-14 16:34:04','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4932,0,'product/BatchAudit','2017-02-14 16:34:09','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4933,0,'product/BatchAudit','2017-02-14 16:34:14','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4934,0,'product/BatchAudit','2017-02-14 16:34:17','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4935,0,'product/BatchAudit','2017-02-14 16:34:21','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4936,0,'product/BatchAudit','2017-02-14 16:34:25','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4937,0,'Category/Add','2017-02-14 16:34:26','admin','120.26.125.164','添加平台分类,操作记录:'),(4938,0,'product/BatchAudit','2017-02-14 16:34:27','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4939,0,'product/BatchAudit','2017-02-14 16:34:30','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4940,1,'/Product/Create','2017-02-14 16:37:01','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=SK-II 神仙水sk2护肤精华露 160ml [成功]'),(4941,1,'/Product/Create','2017-02-14 16:42:45','selleradmin','120.26.125.164','商家发布商品，Id=0, 名称=新西兰蔓越莓蜂蜜480g 进口蜂蜜选新西兰蜂蜜品牌 NatureBeing [成功]'),(4942,0,'product/BatchAudit','2017-02-14 17:25:44','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4943,0,'product/BatchAudit','2017-02-14 17:25:48','admin','120.26.125.164','批量审核商品状态,操作记录:'),(4944,1,'/Product/Edit','2017-02-15 11:20:08','selleradmin','222.240.184.122','商家修改商品，Id=742, 名称=怡鲜来 阿根廷红虾2000g 4斤/盒 大号L2级 40-60尾 [成功]'),(4945,0,'/Shop/Auditing/306','2017-02-15 11:20:49','malldemouser','218.247.15.226','开店审核页面，店铺Id=306,状态为：Open, 说明是：'),(4946,0,'product/BatchAudit','2017-02-15 11:20:58','admin','222.240.184.122','批量审核商品状态,操作记录:'),(4947,0,'Brand/Edit','2017-02-15 14:53:53','admin','222.240.184.122','编辑品牌,操作记录:'),(4948,0,'Brand/Edit','2017-02-15 15:07:04','admin','222.240.184.122','编辑品牌,操作记录:'),(4949,0,'Brand/Edit','2017-02-15 15:07:12','admin','222.240.184.122','编辑品牌,操作记录:'),(4950,0,'Brand/Edit','2017-02-15 15:07:20','admin','222.240.184.122','编辑品牌,操作记录:'),(4951,0,'Brand/Edit','2017-02-15 15:07:39','admin','222.240.184.122','编辑品牌,操作记录:'),(4952,0,'Brand/Edit','2017-02-15 15:07:47','admin','222.240.184.122','编辑品牌,操作记录:'),(4953,0,'Brand/Edit','2017-02-15 15:07:54','admin','222.240.184.122','编辑品牌,操作记录:'),(4954,0,'Brand/Edit','2017-02-15 15:08:01','admin','222.240.184.122','编辑品牌,操作记录:'),(4955,0,'Brand/Edit','2017-02-15 15:08:21','admin','222.240.184.122','编辑品牌,操作记录:'),(4956,0,'Brand/Edit','2017-02-15 15:08:29','admin','222.240.184.122','编辑品牌,操作记录:'),(4957,0,'Brand/Edit','2017-02-15 15:08:36','admin','222.240.184.122','编辑品牌,操作记录:'),(4958,0,'Brand/Edit','2017-02-15 15:08:47','admin','222.240.184.122','编辑品牌,操作记录:'),(4959,0,'Brand/Edit','2017-02-15 15:09:48','admin','222.240.184.122','编辑品牌,操作记录:'),(4960,0,'Brand/Edit','2017-02-15 15:09:56','admin','222.240.184.122','编辑品牌,操作记录:'),(4961,0,'Brand/Edit','2017-02-15 15:10:04','admin','222.240.184.122','编辑品牌,操作记录:'),(4962,0,'Brand/Edit','2017-02-15 15:10:12','admin','222.240.184.122','编辑品牌,操作记录:'),(4963,0,'Brand/Edit','2017-02-15 15:10:19','admin','222.240.184.122','编辑品牌,操作记录:'),(4964,0,'Brand/Edit','2017-02-15 15:10:37','admin','222.240.184.122','编辑品牌,操作记录:'),(4965,0,'Brand/Edit','2017-02-15 15:10:44','admin','222.240.184.122','编辑品牌,操作记录:'),(4966,0,'Brand/Edit','2017-02-15 15:10:53','admin','222.240.184.122','编辑品牌,操作记录:'),(4967,0,'Brand/Edit','2017-02-15 15:11:01','admin','222.240.184.122','编辑品牌,操作记录:'),(4968,0,'Brand/Edit','2017-02-15 15:11:10','admin','222.240.184.122','编辑品牌,操作记录:'),(4969,0,'Brand/Edit','2017-02-15 15:11:19','admin','222.240.184.122','编辑品牌,操作记录:'),(4970,0,'Brand/Edit','2017-02-15 15:11:28','admin','222.240.184.122','编辑品牌,操作记录:'),(4971,0,'Brand/Edit','2017-02-15 15:11:37','admin','222.240.184.122','编辑品牌,操作记录:'),(4972,0,'Brand/Edit','2017-02-15 15:11:46','admin','222.240.184.122','编辑品牌,操作记录:'),(4973,0,'Brand/Edit','2017-02-15 15:11:55','admin','222.240.184.122','编辑品牌,操作记录:'),(4974,0,'Brand/Edit','2017-02-15 15:12:55','admin','222.240.184.122','编辑品牌,操作记录:'),(4975,0,'Brand/Edit','2017-02-15 15:13:05','admin','222.240.184.122','编辑品牌,操作记录:'),(4976,0,'Brand/Edit','2017-02-15 15:13:17','admin','222.240.184.122','编辑品牌,操作记录:'),(4977,0,'Brand/Edit','2017-02-15 15:13:28','admin','222.240.184.122','编辑品牌,操作记录:'),(4978,0,'Brand/Edit','2017-02-15 15:13:41','admin','222.240.184.122','编辑品牌,操作记录:'),(4979,0,'Brand/Edit','2017-02-15 15:13:51','admin','222.240.184.122','编辑品牌,操作记录:'),(4980,0,'Brand/Edit','2017-02-15 15:14:00','admin','222.240.184.122','编辑品牌,操作记录:'),(4981,1,'Manager/Add','2017-02-15 17:15:05','selleradmin','222.240.184.122','添加卖家子帐号,操作记录:'),(4982,1,'Manager/Change','2017-02-15 17:16:35','selleradmin','222.240.184.122','修改商家管理员,操作记录:'),(4983,0,'ProductType/SaveModel','2017-02-16 11:00:45','admin','222.240.184.122','修改平台类目,操作记录:id:88 '),(4984,1,'/Product/Edit','2017-02-16 11:04:44','selleradmin','222.240.184.122','商家修改商品，Id=731, 名称=蔓斯菲尔 设计师椅 简约时尚休闲塑料椅 创意电脑椅子 办公餐椅 会议椅 [成功]'),(4985,1,'/Product/Edit','2017-02-16 11:05:53','selleradmin','222.240.184.122','商家修改商品，Id=731, 名称=蔓斯菲尔 设计师椅 简约时尚休闲塑料椅 创意电脑椅子 办公餐椅 会议椅 [成功]'),(4986,1,'/Product/Edit','2017-02-16 11:06:55','selleradmin','222.240.184.122','商家修改商品，Id=731, 名称=蔓斯菲尔 设计师椅 简约时尚休闲塑料椅 创意电脑椅子 办公餐椅 会议椅 [成功]'),(4987,0,'product/BatchAudit','2017-02-16 11:07:33','admin','222.240.184.122','批量审核商品状态,操作记录:'),(4988,0,'ProductType/SaveModel','2017-02-16 11:11:06','admin','222.240.184.122','修改平台类目,操作记录:id:88 '),(4989,1,'/Product/Edit','2017-02-16 11:12:11','selleradmin','222.240.184.122','商家修改商品，Id=732, 名称=全实木电脑桌书桌办公桌 [成功]'),(4990,0,'product/BatchAudit','2017-02-16 11:12:22','admin','222.240.184.122','批量审核商品状态,操作记录:'),(4991,1,'/Product/Create','2017-02-16 11:24:44','selleradmin','222.240.184.122','商家发布商品，Id=0, 名称=棉麻布艺禅修加厚加大榻榻米地板圆形坐垫飘窗禅修瑜伽打坐垫55CM [成功]'),(4992,0,'product/BatchAudit','2017-02-16 11:24:55','admin','222.240.184.122','批量审核商品状态,操作记录:'),(4993,0,'Gift/Edit','2017-02-16 11:27:40','admin','222.240.184.122','操作礼品信息,操作记录:'),(4994,0,'Gift/Edit','2017-02-16 11:31:05','admin','222.240.184.122','操作礼品信息,操作记录:'),(4995,0,'Gift/Edit','2017-02-16 11:34:29','admin','222.240.184.122','操作礼品信息,操作记录:'),(4996,1,'/Product/Create','2017-02-16 11:35:03','selleradmin','222.240.184.122','商家发布商品，Id=0, 名称=北欧全实木床 简约美式实木北欧家具套装 [成功]'),(4997,0,'product/BatchAudit','2017-02-16 11:35:10','admin','222.240.184.122','批量审核商品状态,操作记录:'),(4998,0,'Gift/Edit','2017-02-16 11:36:13','admin','222.240.184.122','操作礼品信息,操作记录:'),(4999,0,'Gift/Edit','2017-02-16 11:38:23','admin','222.240.184.122','操作礼品信息,操作记录:'),(5000,0,'Gift/Edit','2017-02-16 11:38:30','admin','222.240.184.122','操作礼品信息,操作记录:'),(5001,0,'Gift/Edit','2017-02-16 11:40:50','admin','222.240.184.122','操作礼品信息,操作记录:'),(5002,0,'Gift/Edit','2017-02-16 11:42:59','admin','222.240.184.122','操作礼品信息,操作记录:'),(5003,0,'Gift/Edit','2017-02-16 11:45:13','admin','222.240.184.122','操作礼品信息,操作记录:'),(5004,1,'/Product/Create','2017-02-16 11:46:28','selleradmin','222.240.184.122','商家发布商品，Id=0, 名称=多功能 无网研磨 易清洗 全钢 304不锈钢 豆浆机 [成功]'),(5005,0,'product/BatchAudit','2017-02-16 11:46:50','admin','222.240.184.122','批量审核商品状态,操作记录:'),(5006,0,'Category/Add','2017-02-16 11:55:00','admin','222.240.184.122','添加平台分类,操作记录:'),(5007,0,'Category/Add','2017-02-16 11:55:17','admin','222.240.184.122','添加平台分类,操作记录:'),(5008,1,'/Product/Create','2017-02-16 12:01:21','selleradmin','222.240.184.122','商家发布商品，Id=0, 名称=积家（Jaeger）Master Control大师系列机械男表Q1552520 银色 [成功]'),(5009,0,'product/BatchAudit','2017-02-16 12:01:32','admin','222.240.184.122','批量审核商品状态,操作记录:'),(5010,0,'/Shop/Auditing/308','2017-02-16 13:49:15','admin','222.240.184.122','开店审核页面，店铺Id=308,状态为：Open, 说明是：'),(5011,0,'/Shop/EditPersonal/308','2017-02-16 13:50:37','admin','222.240.184.122','修改店铺信息，Id=308'),(5012,0,'/Shop/Auditing/309','2017-02-16 13:56:18','admin','222.240.184.122','开店审核页面，店铺Id=309,状态为：Open, 说明是：'),(5013,0,'/Shop/EditPersonal/309','2017-02-16 13:56:30','admin','222.240.184.122','修改店铺信息，Id=309'),(5014,1,'/Product/Edit','2017-02-16 15:36:21','selleradmin','222.240.184.122','商家修改商品，Id=740, 名称=红霞草莓 3斤 单果20-30克 新鲜水果 SG [成功]'),(5015,0,'product/BatchAudit','2017-02-16 15:36:34','admin','222.240.184.122','批量审核商品状态,操作记录:'),(5016,1,'/Product/Edit','2017-02-16 15:44:08','selleradmin','222.240.184.122','商家修改商品，Id=742, 名称=怡鲜来 阿根廷红虾2000g 4斤/盒 大号L2级 40-60尾 [成功]'),(5017,0,'product/BatchAudit','2017-02-16 15:44:16','admin','222.240.184.122','批量审核商品状态,操作记录:'),(5018,1,'Navigation/Add','2017-02-16 17:51:08','selleradmin','222.240.184.122','新增导航,操作记录:'),(5019,1,'Navigation/Add','2017-02-16 17:51:31','selleradmin','222.240.184.122','新增导航,操作记录:'),(5020,1,'Navigation/Edit','2017-02-16 17:52:02','selleradmin','222.240.184.122','编辑导航,操作记录:'),(5021,1,'Navigation/Add','2017-02-16 17:52:56','selleradmin','222.240.184.122','新增导航,操作记录:'),(5022,1,'Navigation/Add','2017-02-16 17:53:27','selleradmin','222.240.184.122','新增导航,操作记录:'),(5023,1,'Navigation/Add','2017-02-16 17:53:52','selleradmin','222.240.184.122','新增导航,操作记录:'),(5024,1,'Navigation/Add','2017-02-16 17:54:19','selleradmin','222.240.184.122','新增导航,操作记录:');
/*!40000 ALTER TABLE `mall_log` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_manager`
--

DROP TABLE IF EXISTS `mall_manager`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_manager` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL,
  `RoleId` bigint(20) NOT NULL COMMENT '角色ID',
  `UserName` varchar(100) NOT NULL COMMENT '用户名称',
  `Password` varchar(100) NOT NULL COMMENT '密码',
  `PasswordSalt` varchar(100) NOT NULL COMMENT '密码加盐',
  `CreateDate` datetime NOT NULL COMMENT '创建日期',
  `Remark` varchar(1000) DEFAULT NULL,
  `RealName` varchar(1000) DEFAULT NULL COMMENT '真实名称',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=460 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_manager`
--

LOCK TABLES `mall_manager` WRITE;
/*!40000 ALTER TABLE `mall_manager` DISABLE KEYS */;
INSERT INTO `mall_manager` VALUES (383,1,49,'selleradmin:seller','00c0f7cc4f3f86b177452d4404ca684c','31090574-abf6-489b-bb34-22049d1ab800','2017-02-15 17:15:05','','商家'),(458,1,0,'ziying','e95142368d16e7043c3d5b91c378f2c5','0937fba7-ca5f-4107-94eb-352c9800033f','2018-11-16 16:51:46',NULL,NULL),(459,0,0,'admin','e95142368d16e7043c3d5b91c378f2c5','0937fba7-ca5f-4107-94eb-352c9800033f','2018-11-16 16:51:46',NULL,NULL);
/*!40000 ALTER TABLE `mall_manager` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_marketservicerecord`
--

DROP TABLE IF EXISTS `mall_marketservicerecord`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_marketservicerecord` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `MarketServiceId` bigint(20) NOT NULL,
  `StartTime` datetime NOT NULL COMMENT '开始时间',
  `EndTime` datetime NOT NULL COMMENT '结束时间',
  `BuyTime` datetime NOT NULL COMMENT '购买时间',
  `SettlementFlag` int(16) unsigned zerofill NOT NULL,
  `Price` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '服务购买价格',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_mall_MarketServiceRecord_mall_ActiveMarketService` (`MarketServiceId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=276 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_marketservicerecord`
--

LOCK TABLES `mall_marketservicerecord` WRITE;
/*!40000 ALTER TABLE `mall_marketservicerecord` DISABLE KEYS */;
INSERT INTO `mall_marketservicerecord` VALUES (101,38,'2017-02-14 11:21:28','2018-02-14 11:21:28','2017-02-14 11:21:28',0000000000000001,0.00),(102,38,'2018-02-14 11:21:28','2019-02-14 11:21:28','2017-02-14 11:21:37',0000000000000001,0.00),(103,39,'2017-02-14 11:22:31','2018-02-14 11:22:31','2017-02-14 11:22:31',0000000000000001,0.00),(104,40,'2017-02-14 11:35:34','2017-03-14 11:35:34','2017-02-14 11:35:34',0000000000000001,0.00),(105,41,'2017-02-14 11:38:30','2018-02-14 11:38:30','2017-02-14 11:38:30',0000000000000001,0.00),(106,42,'2017-02-14 11:55:57','2018-02-14 11:55:57','2017-02-14 11:55:57',0000000000000001,0.00),(107,42,'2018-02-14 11:55:57','2019-02-14 11:55:57','2017-02-14 11:56:02',0000000000000001,0.00);
/*!40000 ALTER TABLE `mall_marketservicerecord` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_marketsetting`
--

DROP TABLE IF EXISTS `mall_marketsetting`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_marketsetting` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `TypeId` int(11) NOT NULL COMMENT '营销类型ID',
  `Price` decimal(18,2) NOT NULL COMMENT '营销使用价格（/月）',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_marketsetting`
--

LOCK TABLES `mall_marketsetting` WRITE;
/*!40000 ALTER TABLE `mall_marketsetting` DISABLE KEYS */;
INSERT INTO `mall_marketsetting` VALUES (7,5,0.00),(8,1,0.00),(9,4,0.00),(10,2,0.00),(11,3,0.00);
/*!40000 ALTER TABLE `mall_marketsetting` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_marketsettingmeta`
--

DROP TABLE IF EXISTS `mall_marketsettingmeta`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_marketsettingmeta` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `MarketId` int(11) NOT NULL,
  `MetaKey` varchar(100) NOT NULL,
  `MetaValue` text,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Hiamll_MarketSettingMeta_ToSetting` (`MarketId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_marketsettingmeta`
--

LOCK TABLES `mall_marketsettingmeta` WRITE;
/*!40000 ALTER TABLE `mall_marketsettingmeta` DISABLE KEYS */;
INSERT INTO `mall_marketsettingmeta` VALUES (5,8,'category','洗面奶,电子产品,服装,食品');
/*!40000 ALTER TABLE `mall_marketsettingmeta` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_member`
--

DROP TABLE IF EXISTS `mall_member`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_member` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `UserName` varchar(100) NOT NULL COMMENT '名称',
  `Password` varchar(100) NOT NULL COMMENT '密码',
  `PasswordSalt` varchar(100) NOT NULL COMMENT '密码加盐',
  `Nick` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Sex` int(11) NOT NULL DEFAULT '0' COMMENT '性别',
  `Email` varchar(100) DEFAULT NULL COMMENT '邮件',
  `CreateDate` datetime NOT NULL COMMENT '创建日期',
  `TopRegionId` int(11) NOT NULL COMMENT '省份ID',
  `RegionId` int(11) NOT NULL COMMENT '省市区ID',
  `RealName` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CellPhone` varchar(100) DEFAULT NULL COMMENT '电话',
  `QQ` varchar(100) DEFAULT NULL COMMENT 'QQ',
  `Address` varchar(100) DEFAULT NULL COMMENT '街道地址',
  `Disabled` tinyint(1) NOT NULL COMMENT '是否禁用',
  `LastLoginDate` datetime NOT NULL COMMENT '最后登录日期',
  `OrderNumber` int(11) NOT NULL COMMENT '下单次数',
  `TotalAmount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '总消费金额（不排除退款）',
  `Expenditure` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '总消费金额（不排除退款）',
  `Points` int(11) NOT NULL,
  `Photo` varchar(100) DEFAULT NULL COMMENT '头像',
  `ParentSellerId` bigint(20) NOT NULL DEFAULT '0' COMMENT '商家父账号ID',
  `Remark` varchar(1000) DEFAULT NULL,
  `PayPwd` varchar(100) DEFAULT NULL COMMENT '支付密码',
  `PayPwdSalt` varchar(100) DEFAULT NULL COMMENT '支付密码加密字符',
  `InviteUserId` bigint(20) NOT NULL DEFAULT '0',
  `BirthDay` date DEFAULT NULL COMMENT '会员生日',
  `Occupation` varchar(15) DEFAULT NULL COMMENT '职业',
  `NetAmount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '净消费金额（排除退款）',
  `LastConsumptionTime` datetime DEFAULT NULL COMMENT '最后消费时间',
  `Platform` int(11) NOT NULL DEFAULT '0' COMMENT '用户来源终端',
  PRIMARY KEY (`Id`) USING BTREE,
  UNIQUE KEY `IX_UserName` (`UserName`) USING BTREE,
  KEY `IX_Email` (`Email`) USING BTREE,
  KEY `IX_CellPhone` (`CellPhone`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=754 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_member`
--

LOCK TABLES `mall_member` WRITE;
/*!40000 ALTER TABLE `mall_member` DISABLE KEYS */;
INSERT INTO `mall_member` VALUES (569,'ziying','e95142368d16e7043c3d5b91c378f2c5','0937fba7-ca5f-4107-94eb-352c9800033f',NULL,0,NULL,'2018-11-16 16:51:46',0,0,NULL,NULL,NULL,NULL,0,'2018-11-16 16:51:46',0,0.00,0.00,0,NULL,0,NULL,NULL,NULL,0,NULL,NULL,0.00,NULL,0);
/*!40000 ALTER TABLE `mall_member` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_memberactivitydegree`
--

DROP TABLE IF EXISTS `mall_memberactivitydegree`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_memberactivitydegree` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `UserId` bigint(20) NOT NULL DEFAULT '0' COMMENT '会员编号',
  `OneMonth` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否为一个月活跃用户',
  `ThreeMonth` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否为三个月活跃用户',
  `SixMonth` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否为六个月活跃用户',
  `OneMonthEffectiveTime` datetime DEFAULT NULL COMMENT '一个月活跃会员有效时间',
  `ThreeMonthEffectiveTime` datetime DEFAULT NULL COMMENT '三个月活跃会员有效时间',
  `SixMonthEffectiveTime` datetime DEFAULT NULL COMMENT '六个月活跃会员有效时间',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_memberactivitydegree`
--

LOCK TABLES `mall_memberactivitydegree` WRITE;
/*!40000 ALTER TABLE `mall_memberactivitydegree` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_memberactivitydegree` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_memberbuycategory`
--

DROP TABLE IF EXISTS `mall_memberbuycategory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_memberbuycategory` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `UserId` bigint(20) NOT NULL COMMENT '会员ID',
  `CategoryId` bigint(20) NOT NULL COMMENT '类别ID',
  `OrdersCount` int(11) NOT NULL DEFAULT '0' COMMENT '购买次数',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=198 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_memberbuycategory`
--

LOCK TABLES `mall_memberbuycategory` WRITE;
/*!40000 ALTER TABLE `mall_memberbuycategory` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_memberbuycategory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_memberconsumestatistic`
--

DROP TABLE IF EXISTS `mall_memberconsumestatistic`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_memberconsumestatistic` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `UserId` bigint(20) NOT NULL,
  `ShopId` bigint(20) NOT NULL COMMENT '门店Id',
  `NetAmount` decimal(10,2) NOT NULL COMMENT '净消费金额(退款需要维护)',
  `OrderNumber` bigint(20) NOT NULL COMMENT '消费次数(退款不维护)',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_memberconsumestatistic`
--

LOCK TABLES `mall_memberconsumestatistic` WRITE;
/*!40000 ALTER TABLE `mall_memberconsumestatistic` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_memberconsumestatistic` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_membercontact`
--

DROP TABLE IF EXISTS `mall_membercontact`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_membercontact` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `UserId` bigint(20) NOT NULL COMMENT '用户ID',
  `UserType` int(11) NOT NULL COMMENT '用户类型(0 Email  1 SMS)',
  `ServiceProvider` varchar(100) NOT NULL COMMENT '插件名称',
  `Contact` varchar(100) NOT NULL COMMENT '联系号码',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=371 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_membercontact`
--

LOCK TABLES `mall_membercontact` WRITE;
/*!40000 ALTER TABLE `mall_membercontact` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_membercontact` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_membergrade`
--

DROP TABLE IF EXISTS `mall_membergrade`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_membergrade` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `GradeName` varchar(100) NOT NULL COMMENT '会员等级名称',
  `Integral` int(11) NOT NULL COMMENT '该等级所需积分',
  `Remark` varchar(1000) DEFAULT NULL COMMENT '描述',
  `Discount` decimal(8,2) NOT NULL DEFAULT '10.00',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_membergrade`
--

LOCK TABLES `mall_membergrade` WRITE;
/*!40000 ALTER TABLE `mall_membergrade` DISABLE KEYS */;
INSERT INTO `mall_membergrade` VALUES (6,'普通会员',0,NULL,9.00),(7,'黄金会员',5000,NULL,8.00);
/*!40000 ALTER TABLE `mall_membergrade` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_membergroup`
--

DROP TABLE IF EXISTS `mall_membergroup`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_membergroup` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT 'Id',
  `ShopId` bigint(20) NOT NULL DEFAULT '0' COMMENT '门店编号',
  `StatisticsType` int(11) NOT NULL COMMENT '统计类型',
  `Total` int(11) NOT NULL COMMENT '统计数量',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_membergroup`
--

LOCK TABLES `mall_membergroup` WRITE;
/*!40000 ALTER TABLE `mall_membergroup` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_membergroup` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_memberintegral`
--

DROP TABLE IF EXISTS `mall_memberintegral`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_memberintegral` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `MemberId` bigint(20) NOT NULL COMMENT '会员ID',
  `UserName` varchar(100) NOT NULL COMMENT '用户名称',
  `HistoryIntegrals` int(11) NOT NULL COMMENT '用户历史积分',
  `AvailableIntegrals` int(11) NOT NULL COMMENT '用户可用积分',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Member_MemberIntegral` (`MemberId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=278 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_memberintegral`
--

LOCK TABLES `mall_memberintegral` WRITE;
/*!40000 ALTER TABLE `mall_memberintegral` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_memberintegral` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_memberintegralexchangerule`
--

DROP TABLE IF EXISTS `mall_memberintegralexchangerule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_memberintegralexchangerule` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `IntegralPerMoney` int(11) NOT NULL COMMENT '一块钱对应多少积分',
  `MoneyPerIntegral` int(11) NOT NULL COMMENT '一个积分对应多少钱',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_memberintegralexchangerule`
--

LOCK TABLES `mall_memberintegralexchangerule` WRITE;
/*!40000 ALTER TABLE `mall_memberintegralexchangerule` DISABLE KEYS */;
INSERT INTO `mall_memberintegralexchangerule` VALUES (2,100,0);
/*!40000 ALTER TABLE `mall_memberintegralexchangerule` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_memberintegralrecord`
--

DROP TABLE IF EXISTS `mall_memberintegralrecord`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_memberintegralrecord` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `MemberId` bigint(20) NOT NULL,
  `UserName` varchar(100) NOT NULL COMMENT '用户名称',
  `TypeId` int(11) NOT NULL COMMENT '兑换类型（登录、下单等）',
  `Integral` int(11) NOT NULL COMMENT '积分数量',
  `RecordDate` datetime NOT NULL COMMENT '记录日期',
  `ReMark` varchar(100) DEFAULT NULL COMMENT '说明',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `fk_MemberId_Members` (`MemberId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=3758 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_memberintegralrecord`
--

LOCK TABLES `mall_memberintegralrecord` WRITE;
/*!40000 ALTER TABLE `mall_memberintegralrecord` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_memberintegralrecord` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_memberintegralrecordaction`
--

DROP TABLE IF EXISTS `mall_memberintegralrecordaction`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_memberintegralrecordaction` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `IntegralRecordId` bigint(20) NOT NULL COMMENT '积分兑换ID',
  `VirtualItemTypeId` int(11) NOT NULL COMMENT '兑换虚拟物l类型ID',
  `VirtualItemId` bigint(20) NOT NULL COMMENT '虚拟物ID',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `fk_IntegralRecordId_MemberIntegralRecord` (`IntegralRecordId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=1241 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_memberintegralrecordaction`
--

LOCK TABLES `mall_memberintegralrecordaction` WRITE;
/*!40000 ALTER TABLE `mall_memberintegralrecordaction` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_memberintegralrecordaction` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_memberintegralrule`
--

DROP TABLE IF EXISTS `mall_memberintegralrule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_memberintegralrule` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `TypeId` int(11) NOT NULL COMMENT '积分规则类型ID',
  `Integral` int(11) NOT NULL COMMENT '规则对应的积分数量',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_memberintegralrule`
--

LOCK TABLES `mall_memberintegralrule` WRITE;
/*!40000 ALTER TABLE `mall_memberintegralrule` DISABLE KEYS */;
INSERT INTO `mall_memberintegralrule` VALUES (1,5,0),(2,6,0),(3,7,0),(4,9,0);
/*!40000 ALTER TABLE `mall_memberintegralrule` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_memberlabel`
--

DROP TABLE IF EXISTS `mall_memberlabel`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_memberlabel` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT 'Id',
  `MemId` bigint(20) NOT NULL COMMENT '会员ID',
  `LabelId` bigint(20) NOT NULL COMMENT '标签Id',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=455 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_memberlabel`
--

LOCK TABLES `mall_memberlabel` WRITE;
/*!40000 ALTER TABLE `mall_memberlabel` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_memberlabel` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_memberopenid`
--

DROP TABLE IF EXISTS `mall_memberopenid`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_memberopenid` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `UserId` bigint(20) NOT NULL COMMENT '用户ID',
  `OpenId` varchar(100) DEFAULT NULL COMMENT '微信OpenID',
  `UnionOpenId` varchar(100) DEFAULT NULL COMMENT '开发平台Openid',
  `UnionId` varchar(100) DEFAULT NULL COMMENT '开发平台Unionid',
  `ServiceProvider` varchar(100) NOT NULL COMMENT '插件名称（mall.Plugin.OAuth.WeiXin）',
  `AppIdType` int(255) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Member_MemberOpenId` (`UserId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=803 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_memberopenid`
--

LOCK TABLES `mall_memberopenid` WRITE;
/*!40000 ALTER TABLE `mall_memberopenid` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_memberopenid` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_membersignin`
--

DROP TABLE IF EXISTS `mall_membersignin`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_membersignin` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `UserId` bigint(20) NOT NULL COMMENT '用户ID',
  `LastSignTime` datetime NOT NULL COMMENT '最近签到时间',
  `DurationDay` int(11) NOT NULL DEFAULT '0' COMMENT '持续签到天数 每周期后清零',
  `DurationDaySum` int(11) NOT NULL DEFAULT '0' COMMENT '持续签到天数总数 非连续周期清零',
  `SignDaySum` bigint(20) NOT NULL DEFAULT '0' COMMENT '签到总天数',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `IDX_mall_MenIn_UserId` (`UserId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_membersignin`
--

LOCK TABLES `mall_membersignin` WRITE;
/*!40000 ALTER TABLE `mall_membersignin` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_membersignin` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_menu`
--

DROP TABLE IF EXISTS `mall_menu`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_menu` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ParentId` bigint(20) NOT NULL COMMENT '上级ID',
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `Title` varchar(10) NOT NULL COMMENT '标题',
  `Url` varchar(200) DEFAULT NULL COMMENT '链接地址',
  `Depth` smallint(6) NOT NULL COMMENT '深度',
  `Sequence` smallint(6) NOT NULL,
  `FullIdPath` varchar(100) NOT NULL COMMENT '全路径',
  `Platform` int(11) NOT NULL COMMENT '终端',
  `UrlType` int(11) NOT NULL COMMENT 'url类型',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=73 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_menu`
--

LOCK TABLES `mall_menu` WRITE;
/*!40000 ALTER TABLE `mall_menu` DISABLE KEYS */;
INSERT INTO `mall_menu` VALUES (65,0,0,'商城','',1,1,'1',1,0),(66,0,0,'全员开店','./m-wap/home/RecruitPlan',1,1,'1',1,6),(67,0,0,'活动中心','',1,1,'1',1,0),(68,67,0,'大转盘','./m-Mobile/BigWheel/index/155',2,1,'1',1,6),(69,67,0,'刮刮卡','./m-Mobile/ScratchCard/index/154',2,1,'1',1,6),(70,67,0,'签到','./m-wap/SignIn/',2,1,'1',1,6),(71,65,0,'首页','./m-WeiXin/',2,1,'1',1,1),(72,65,0,'微店','./m-WeiXin/vshop/list',2,1,'1',1,2);
/*!40000 ALTER TABLE `mall_menu` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_messagelog`
--

DROP TABLE IF EXISTS `mall_messagelog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_messagelog` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL,
  `TypeId` varchar(100) DEFAULT NULL,
  `MessageContent` varchar(1000) DEFAULT NULL,
  `SendTime` datetime NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2831 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_messagelog`
--

LOCK TABLES `mall_messagelog` WRITE;
/*!40000 ALTER TABLE `mall_messagelog` DISABLE KEYS */;
INSERT INTO `mall_messagelog` VALUES (875,0,'短信','尊','2017-02-14 10:16:37'),(876,0,'短信','尊','2017-02-15 11:19:31'),(877,0,'短信','尊','2017-02-15 19:00:22'),(878,0,'短信','尊','2017-02-16 13:41:07');
/*!40000 ALTER TABLE `mall_messagelog` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_mobilefootmenu`
--

DROP TABLE IF EXISTS `mall_mobilefootmenu`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_mobilefootmenu` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) DEFAULT NULL COMMENT '导航名称',
  `Url` varchar(255) DEFAULT NULL COMMENT '链接地址',
  `MenuIcon` varchar(255) DEFAULT NULL COMMENT '显示图片',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=20 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_mobilefootmenu`
--

LOCK TABLES `mall_mobilefootmenu` WRITE;
/*!40000 ALTER TABLE `mall_mobilefootmenu` DISABLE KEYS */;
INSERT INTO `mall_mobilefootmenu` VALUES (9,'首页','/m-wap','/temp/201801151513598392290.png'),(16,'购物车','/m-wap/cart/cart','/temp/201801151516188685020.png'),(17,'个人中心','/m-wap/member/center','/temp/201801151517283011020.png'),(18,'周边门店','/m-wap/shopbranch/storelist','/temp/201801151517094925190.png'),(19,'微店列表','/m-wap/vshop','/temp/201801151514216165760.png');
/*!40000 ALTER TABLE `mall_mobilefootmenu` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_mobilehomeproduct`
--

DROP TABLE IF EXISTS `mall_mobilehomeproduct`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_mobilehomeproduct` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL,
  `PlatFormType` int(11) NOT NULL COMMENT '终端类型(微信、WAP）',
  `Sequence` smallint(6) NOT NULL COMMENT '顺序',
  `ProductId` bigint(20) NOT NULL COMMENT '商品ID',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_mall_MobileHomeProducts_mall_Products` (`ProductId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=188 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_mobilehomeproduct`
--

LOCK TABLES `mall_mobilehomeproduct` WRITE;
/*!40000 ALTER TABLE `mall_mobilehomeproduct` DISABLE KEYS */;
INSERT INTO `mall_mobilehomeproduct` VALUES (104,0,3,1,734),(105,0,3,1,735),(106,0,3,1,736),(107,0,3,1,737),(109,0,3,1,739),(110,0,3,1,740),(111,0,3,1,741),(112,0,3,1,742),(113,0,3,1,743),(114,0,3,1,719),(115,1,1,1,704),(116,1,1,1,705),(117,1,1,1,706),(118,1,1,1,707),(119,1,1,1,708),(120,1,1,1,719),(121,1,1,1,720),(122,1,1,1,721),(123,1,1,1,722),(124,1,1,1,723),(125,1,1,1,731),(126,1,1,1,732),(127,1,1,1,733),(128,1,1,1,734),(129,0,3,1,708),(130,0,3,1,720),(131,0,3,1,721),(132,0,3,1,722),(133,0,3,1,732),(134,0,3,1,733);
/*!40000 ALTER TABLE `mall_mobilehomeproduct` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_mobilehometopic`
--

DROP TABLE IF EXISTS `mall_mobilehometopic`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_mobilehometopic` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL DEFAULT '0' COMMENT '店铺ID',
  `Platform` int(11) NOT NULL COMMENT '终端',
  `TopicId` bigint(20) NOT NULL COMMENT '专题ID',
  `Sequence` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK__mall_Mo__Topic__02C769E9` (`TopicId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_mobilehometopic`
--

LOCK TABLES `mall_mobilehometopic` WRITE;
/*!40000 ALTER TABLE `mall_mobilehometopic` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_mobilehometopic` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_moduleproduct`
--

DROP TABLE IF EXISTS `mall_moduleproduct`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_moduleproduct` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ModuleId` bigint(20) NOT NULL COMMENT '模块ID',
  `ProductId` bigint(20) NOT NULL COMMENT '商品ID',
  `DisplaySequence` bigint(20) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Product_ModuleProduct` (`ProductId`) USING BTREE,
  KEY `FK_TopicModule_ModuleProduct` (`ModuleId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=964 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_moduleproduct`
--

LOCK TABLES `mall_moduleproduct` WRITE;
/*!40000 ALTER TABLE `mall_moduleproduct` DISABLE KEYS */;
INSERT INTO `mall_moduleproduct` VALUES (723,213,706,1),(724,213,707,2),(725,213,708,3),(726,213,709,4),(727,213,710,5),(728,213,711,6),(729,213,712,7),(730,213,713,8),(731,214,732,1),(732,214,733,2),(733,214,734,3),(734,215,716,1),(735,215,740,2),(736,215,742,3),(737,216,708,1),(738,216,709,2),(739,216,710,3),(740,216,711,4),(741,216,715,5),(742,216,716,6),(743,216,717,7),(744,216,718,8),(745,217,708,1),(746,217,709,2),(747,217,710,3),(748,217,711,4),(749,217,715,5),(750,217,716,6),(751,217,717,7),(752,217,718,8),(753,218,746,1),(754,218,747,2),(755,219,714,1),(756,219,715,2),(757,219,716,3),(758,219,718,4),(759,220,709,1),(760,220,710,2),(761,220,712,3),(762,220,713,4),(763,221,699,1),(764,221,700,2),(765,221,701,3),(766,221,702,4),(837,245,705,1),(838,245,706,2),(839,245,707,3),(840,245,708,4),(841,245,709,5),(842,245,710,6),(843,246,699,1),(844,246,700,2),(845,246,701,3),(846,246,702,4),(847,246,703,5),(860,250,699,1),(861,250,700,2),(862,250,701,3),(863,250,702,4),(864,251,705,1),(865,251,706,2),(866,251,707,3),(867,251,708,4),(868,252,732,1),(869,252,733,2),(870,252,746,3),(871,252,747,4),(905,260,731,1),(906,260,732,2),(907,260,733,3),(908,260,734,4),(909,260,735,5),(910,260,746,6),(911,260,747,7),(912,260,748,8),(926,264,699,1),(927,264,700,2),(928,264,701,3),(929,264,702,4),(930,265,704,1),(931,265,706,2),(932,265,707,3),(933,265,708,4),(934,266,732,1),(935,266,733,2),(936,266,746,3),(937,266,747,4),(938,267,699,1),(939,267,700,2),(940,267,701,3),(941,267,702,4),(942,268,704,1),(943,268,706,2),(944,268,707,3),(945,268,708,4),(946,269,732,1),(947,269,733,2),(948,269,734,3),(949,269,747,4),(957,273,746,1),(958,273,747,2),(959,274,740,1),(960,274,742,2),(961,274,745,3),(962,275,736,1),(963,275,738,2);
/*!40000 ALTER TABLE `mall_moduleproduct` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_openid`
--

DROP TABLE IF EXISTS `mall_openid`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_openid` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `OpenId` varchar(100) NOT NULL,
  `SubscribeTime` date NOT NULL COMMENT '关注时间',
  `IsSubscribe` tinyint(1) NOT NULL COMMENT '是否关注',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_openid`
--

LOCK TABLES `mall_openid` WRITE;
/*!40000 ALTER TABLE `mall_openid` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_openid` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_order`
--

DROP TABLE IF EXISTS `mall_order`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_order` (
  `Id` bigint(20) NOT NULL,
  `OrderStatus` int(11) NOT NULL COMMENT '订单状态 [Description("待付款")]WaitPay = 1,[Description("待发货")]WaitDelivery,[Description("待收货")]WaitReceiving,[Description("已关闭")]Close,[Description("已完成")]Finish',
  `OrderDate` datetime NOT NULL COMMENT '订单创建日期',
  `CloseReason` varchar(1000) DEFAULT NULL COMMENT '关闭原因',
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `ShopName` varchar(100) NOT NULL COMMENT '店铺名称',
  `SellerPhone` varchar(100) DEFAULT NULL COMMENT '商家电话',
  `SellerAddress` varchar(100) DEFAULT NULL COMMENT '商家发货地址',
  `SellerRemark` varchar(1000) DEFAULT NULL COMMENT '商家说明',
  `SellerRemarkFlag` int(11) DEFAULT NULL,
  `UserId` bigint(20) NOT NULL COMMENT '会员ID',
  `UserName` varchar(100) NOT NULL COMMENT '会员名称',
  `UserRemark` varchar(1000) DEFAULT NULL COMMENT '会员留言',
  `ShipTo` varchar(100) NOT NULL COMMENT '收货人',
  `CellPhone` varchar(100) DEFAULT NULL COMMENT '收货人电话',
  `TopRegionId` int(11) NOT NULL COMMENT '收货人地址省份ID',
  `RegionId` int(11) NOT NULL COMMENT '收货人区域ID',
  `RegionFullName` varchar(100) NOT NULL COMMENT '全名的收货地址',
  `Address` varchar(100) NOT NULL COMMENT '收货具体街道信息',
  `ReceiveLongitude` float NOT NULL DEFAULT '0' COMMENT '收货地址坐标',
  `ReceiveLatitude` float NOT NULL DEFAULT '0' COMMENT '收货地址坐标',
  `ExpressCompanyName` varchar(100) DEFAULT NULL COMMENT '快递公司',
  `Freight` decimal(8,2) NOT NULL COMMENT '运费',
  `ShipOrderNumber` varchar(100) CHARACTER SET utf8mb4 DEFAULT NULL COMMENT '物流订单号',
  `ShippingDate` datetime DEFAULT NULL COMMENT '发货日期',
  `IsPrinted` tinyint(1) NOT NULL COMMENT '是否打印快递单',
  `PaymentTypeName` varchar(100) DEFAULT NULL COMMENT '付款类型名称',
  `PaymentTypeGateway` varchar(100) DEFAULT NULL COMMENT '付款类型使用 插件名称',
  `PaymentType` int(11) NOT NULL,
  `GatewayOrderId` varchar(100) DEFAULT NULL COMMENT '支付接口返回的ID',
  `PayRemark` varchar(1000) DEFAULT NULL COMMENT '付款注释',
  `PayDate` datetime DEFAULT NULL COMMENT '付款日期',
  `Tax` decimal(8,2) NOT NULL COMMENT '税钱，但是未使用',
  `FinishDate` datetime DEFAULT NULL COMMENT '完成订单日期',
  `ProductTotalAmount` decimal(18,2) NOT NULL COMMENT '商品总金额',
  `RefundTotalAmount` decimal(18,2) NOT NULL COMMENT '退款金额',
  `CommisTotalAmount` decimal(18,2) NOT NULL COMMENT '佣金总金额',
  `RefundCommisAmount` decimal(18,2) NOT NULL COMMENT '退还佣金总金额',
  `ActiveType` int(11) NOT NULL DEFAULT '0' COMMENT '未使用',
  `Platform` int(11) NOT NULL DEFAULT '0' COMMENT '来自哪个终端的订单',
  `DiscountAmount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '针对该订单的优惠金额（用于优惠券）',
  `IntegralDiscount` decimal(18,2) NOT NULL COMMENT '积分优惠金额',
  `OrderType` int(11) NOT NULL DEFAULT '0' COMMENT '订单类型',
  `OrderRemarks` varchar(200) DEFAULT NULL COMMENT '订单备注(买家留言)',
  `LastModifyTime` datetime NOT NULL COMMENT '最后操作时间',
  `DeliveryType` int(11) NOT NULL COMMENT '发货类型(快递配送,到店自提)',
  `ShopBranchId` bigint(20) NOT NULL DEFAULT '0' COMMENT '门店ID',
  `PickupCode` varchar(20) DEFAULT NULL COMMENT '提货码',
  `TotalAmount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '订单实付金额',
  `ActualPayAmount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '订单实收金额',
  `FullDiscount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '满额减金额',
  `CapitalAmount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '预付款支付金额',
  `CouponId` bigint(20) NOT NULL COMMENT '使用的优惠券Id',
  `CancelReason` varchar(200) DEFAULT NULL COMMENT '达达取消发单原因',
  `DadaStatus` int(11) NOT NULL DEFAULT '0' COMMENT '达达状态',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_order`
--

LOCK TABLES `mall_order` WRITE;
/*!40000 ALTER TABLE `mall_order` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_order` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_ordercomment`
--

DROP TABLE IF EXISTS `mall_ordercomment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_ordercomment` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `OrderId` bigint(20) NOT NULL COMMENT '订单ID',
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `ShopName` varchar(100) NOT NULL COMMENT '店铺名称',
  `UserId` bigint(20) NOT NULL COMMENT '用户ID',
  `UserName` varchar(100) NOT NULL COMMENT '用户名称',
  `CommentDate` datetime NOT NULL COMMENT '评价日期',
  `PackMark` int(11) NOT NULL COMMENT '包装评分',
  `DeliveryMark` int(11) NOT NULL COMMENT '物流评分',
  `ServiceMark` int(11) NOT NULL COMMENT '服务评分',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Order_OrderComment` (`OrderId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=742 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_ordercomment`
--

LOCK TABLES `mall_ordercomment` WRITE;
/*!40000 ALTER TABLE `mall_ordercomment` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_ordercomment` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_ordercomplaint`
--

DROP TABLE IF EXISTS `mall_ordercomplaint`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_ordercomplaint` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `OrderId` bigint(20) NOT NULL COMMENT '订单ID',
  `Status` int(11) NOT NULL COMMENT '审核状态',
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `ShopName` varchar(100) NOT NULL COMMENT '店铺名称',
  `ShopPhone` varchar(100) NOT NULL COMMENT '店铺联系方式',
  `UserId` bigint(20) NOT NULL COMMENT '用户ID',
  `UserName` varchar(100) NOT NULL COMMENT '用户名称',
  `UserPhone` varchar(100) DEFAULT NULL COMMENT '用户联系方式',
  `ComplaintDate` datetime NOT NULL COMMENT '投诉日期',
  `ComplaintReason` varchar(1000) NOT NULL COMMENT '投诉原因',
  `SellerReply` varchar(1000) DEFAULT NULL COMMENT '商家反馈信息',
  `PlatRemark` varchar(10000) DEFAULT NULL COMMENT '投诉备注',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Order_OrderComplaint` (`OrderId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=37 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_ordercomplaint`
--

LOCK TABLES `mall_ordercomplaint` WRITE;
/*!40000 ALTER TABLE `mall_ordercomplaint` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_ordercomplaint` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_orderexpressdata`
--

DROP TABLE IF EXISTS `mall_orderexpressdata`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_orderexpressdata` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `CompanyCode` varchar(50) NOT NULL,
  `ExpressNumber` varchar(50) NOT NULL,
  `DataContent` varchar(2000) DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_orderexpressdata`
--

LOCK TABLES `mall_orderexpressdata` WRITE;
/*!40000 ALTER TABLE `mall_orderexpressdata` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_orderexpressdata` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_orderinvoice`
--

DROP TABLE IF EXISTS `mall_orderinvoice`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_orderinvoice` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `OrderId` bigint(20) NOT NULL DEFAULT '0' COMMENT '订单编号',
  `InvoiceType` int(11) NOT NULL DEFAULT '0' COMMENT '发票类型（1:普通发票、2:电子发票、3:增值税发票）',
  `InvoiceTitle` varchar(100) DEFAULT NULL COMMENT '发票抬头',
  `InvoiceCode` varchar(200) DEFAULT NULL COMMENT '税号',
  `InvoiceContext` varchar(100) DEFAULT NULL COMMENT '发票明细(个人、公司)',
  `RegisterAddress` varchar(200) DEFAULT NULL COMMENT '注册地址',
  `RegisterPhone` varchar(50) DEFAULT NULL COMMENT '注册电话',
  `BankName` varchar(100) DEFAULT NULL COMMENT '开户银行',
  `BankNo` varchar(50) DEFAULT NULL COMMENT '银行帐号',
  `RealName` varchar(50) DEFAULT NULL COMMENT '收票人姓名',
  `CellPhone` varchar(20) DEFAULT NULL COMMENT '收票人手机号',
  `Email` varchar(50) DEFAULT NULL COMMENT '收票人邮箱',
  `RegionID` int(11) NOT NULL DEFAULT '0' COMMENT '收票人地址区域ID',
  `Address` varchar(100) DEFAULT NULL COMMENT '收票人详细地址',
  `VatInvoiceDay` int(11) NOT NULL DEFAULT '0' COMMENT '订单完成后多少天开具增值税发票',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_orderinvoice`
--

LOCK TABLES `mall_orderinvoice` WRITE;
/*!40000 ALTER TABLE `mall_orderinvoice` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_orderinvoice` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_orderitem`
--

DROP TABLE IF EXISTS `mall_orderitem`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_orderitem` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '主键',
  `OrderId` bigint(20) NOT NULL COMMENT '订单ID',
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `ProductId` bigint(20) NOT NULL COMMENT '商品ID',
  `SkuId` varchar(100) DEFAULT NULL COMMENT 'SKUId',
  `SKU` varchar(100) DEFAULT NULL COMMENT 'SKU表SKU字段',
  `Quantity` bigint(20) NOT NULL COMMENT '购买数量',
  `ReturnQuantity` bigint(20) NOT NULL COMMENT '退货数量',
  `CostPrice` decimal(18,2) NOT NULL COMMENT '成本价',
  `SalePrice` decimal(18,2) NOT NULL COMMENT '销售价',
  `DiscountAmount` decimal(18,2) NOT NULL COMMENT '优惠金额',
  `RealTotalPrice` decimal(18,2) NOT NULL COMMENT '实际应付金额',
  `RefundPrice` decimal(18,2) NOT NULL COMMENT '退款价格',
  `ProductName` varchar(100) NOT NULL COMMENT '商品名称',
  `Color` varchar(100) DEFAULT NULL COMMENT 'SKU颜色',
  `Size` varchar(100) DEFAULT NULL COMMENT 'SKU尺寸',
  `Version` varchar(100) DEFAULT NULL COMMENT 'SKU版本',
  `ThumbnailsUrl` varchar(100) DEFAULT NULL COMMENT '缩略图',
  `CommisRate` decimal(18,2) NOT NULL COMMENT '分佣比例',
  `EnabledRefundAmount` decimal(18,2) DEFAULT NULL COMMENT '可退金额',
  `IsLimitBuy` tinyint(1) NOT NULL DEFAULT '0' COMMENT '是否为限时购商品',
  `EnabledRefundIntegral` decimal(18,2) DEFAULT NULL COMMENT '可退积分抵扣金额',
  `CouponDiscount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '优惠券抵扣金额',
  `FullDiscount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '满额减平摊到订单项的金额',
  `EffectiveDate` datetime DEFAULT NULL COMMENT '核销码生效时间',
  `FlashSaleId` bigint(11) NOT NULL DEFAULT '0' COMMENT '限时购活动ID',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Order_OrderItem` (`OrderId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=4214 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_orderitem`
--

LOCK TABLES `mall_orderitem` WRITE;
/*!40000 ALTER TABLE `mall_orderitem` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_orderitem` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_orderoperationlog`
--

DROP TABLE IF EXISTS `mall_orderoperationlog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_orderoperationlog` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `OrderId` bigint(20) NOT NULL COMMENT '订单ID',
  `Operator` varchar(100) NOT NULL COMMENT '操作者',
  `OperateDate` datetime NOT NULL COMMENT '操作日期',
  `OperateContent` varchar(1000) DEFAULT NULL COMMENT '操作内容',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Order_OrderOperationLog` (`OrderId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=3198 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_orderoperationlog`
--

LOCK TABLES `mall_orderoperationlog` WRITE;
/*!40000 ALTER TABLE `mall_orderoperationlog` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_orderoperationlog` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_orderpay`
--

DROP TABLE IF EXISTS `mall_orderpay`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_orderpay` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `PayId` bigint(20) NOT NULL,
  `OrderId` bigint(20) NOT NULL,
  `PayState` tinyint(1) unsigned zerofill NOT NULL COMMENT '支付状态',
  `PayTime` datetime DEFAULT NULL COMMENT '支付时间',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2277 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_orderpay`
--

LOCK TABLES `mall_orderpay` WRITE;
/*!40000 ALTER TABLE `mall_orderpay` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_orderpay` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_orderrefund`
--

DROP TABLE IF EXISTS `mall_orderrefund`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_orderrefund` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `OrderId` bigint(20) NOT NULL COMMENT '订单ID',
  `OrderItemId` bigint(20) NOT NULL COMMENT '订单详情ID',
  `VerificationCodeIds` varchar(1000) DEFAULT '' COMMENT '核销码ID集合(本次申请哪些核销码退款)',
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `ShopName` varchar(100) NOT NULL COMMENT '店铺名称',
  `UserId` bigint(20) NOT NULL COMMENT '用户ID',
  `Applicant` varchar(100) NOT NULL COMMENT '申请内容',
  `ContactPerson` varchar(100) DEFAULT NULL COMMENT '联系人',
  `ContactCellPhone` varchar(100) DEFAULT NULL COMMENT '联系电话',
  `RefundAccount` varchar(100) DEFAULT NULL COMMENT '退款金额',
  `ApplyDate` datetime NOT NULL COMMENT '申请时间',
  `Amount` decimal(18,2) NOT NULL COMMENT '金额',
  `Reason` varchar(1000) NOT NULL COMMENT '退款原因',
  `ReasonDetail` varchar(1000) DEFAULT NULL COMMENT '退款详情',
  `SellerAuditStatus` int(11) NOT NULL COMMENT '商家审核状态',
  `SellerAuditDate` datetime NOT NULL COMMENT '商家审核时间',
  `SellerRemark` varchar(1000) DEFAULT NULL COMMENT '商家注释',
  `ManagerConfirmStatus` int(11) NOT NULL COMMENT '平台审核状态',
  `ManagerConfirmDate` datetime NOT NULL COMMENT '平台审核时间',
  `ManagerRemark` varchar(1000) DEFAULT NULL COMMENT '平台注释',
  `IsReturn` tinyint(1) NOT NULL COMMENT '是否已经退款',
  `ExpressCompanyName` varchar(100) DEFAULT NULL COMMENT '快递公司',
  `ShipOrderNumber` varchar(100) DEFAULT NULL COMMENT '快递单号',
  `Payee` varchar(200) DEFAULT NULL COMMENT '收款人',
  `PayeeAccount` varchar(200) DEFAULT NULL COMMENT '收款人账户',
  `RefundMode` int(11) NOT NULL COMMENT '退款方式',
  `RefundPayStatus` int(11) NOT NULL DEFAULT '2' COMMENT '退款支付状态',
  `RefundPayType` int(11) NOT NULL COMMENT '退款支付类型',
  `BuyerDeliverDate` datetime DEFAULT NULL COMMENT '买家发货时间',
  `SellerConfirmArrivalDate` datetime DEFAULT NULL COMMENT '卖家确认到货时间',
  `RefundBatchNo` varchar(30) DEFAULT NULL COMMENT '退款批次号',
  `RefundPostTime` datetime DEFAULT NULL COMMENT '退款异步提交时间',
  `ReturnQuantity` bigint(20) NOT NULL DEFAULT '0' COMMENT '退货数量',
  `ReturnPlatCommission` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '平台佣金退还',
  `ApplyNumber` int(11) NOT NULL COMMENT '申请次数',
  `CertPic1` varchar(200) DEFAULT NULL COMMENT '凭证图片1',
  `CertPic2` varchar(200) DEFAULT NULL COMMENT '凭证图片2',
  `CertPic3` varchar(200) DEFAULT NULL COMMENT '凭证图片3',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_OrderItem_OrderRefund` (`OrderItemId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=728 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_orderrefund`
--

LOCK TABLES `mall_orderrefund` WRITE;
/*!40000 ALTER TABLE `mall_orderrefund` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_orderrefund` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_orderrefundlog`
--

DROP TABLE IF EXISTS `mall_orderrefundlog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_orderrefundlog` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `RefundId` bigint(20) NOT NULL COMMENT '售后编号',
  `Operator` varchar(100) NOT NULL COMMENT '操作者',
  `OperateDate` datetime NOT NULL COMMENT '操作日期',
  `OperateContent` varchar(1000) DEFAULT NULL COMMENT '操作内容',
  `ApplyNumber` int(11) NOT NULL COMMENT '申请次数',
  `Step` smallint(6) NOT NULL COMMENT '退款步聚(枚举:CommonModel.Enum.OrderRefundStep)',
  `Remark` varchar(255) DEFAULT NULL COMMENT '备注(买家留言/商家留言/商家拒绝原因/平台退款备注)',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=1346 DEFAULT CHARSET=utf8 COMMENT='订单售后日志表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_orderrefundlog`
--

LOCK TABLES `mall_orderrefundlog` WRITE;
/*!40000 ALTER TABLE `mall_orderrefundlog` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_orderrefundlog` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_orderverificationcode`
--

DROP TABLE IF EXISTS `mall_orderverificationcode`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_orderverificationcode` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `OrderId` bigint(20) NOT NULL COMMENT '订单ID',
  `OrderItemId` bigint(20) NOT NULL COMMENT '订单项ID',
  `Status` tinyint(4) NOT NULL COMMENT '核销码状态(1=待核销，2=已核销，3=退款中，4=退款完成，5=已过期)',
  `VerificationCode` varchar(15) NOT NULL COMMENT '核销码(12位随机数)',
  `VerificationTime` datetime DEFAULT NULL COMMENT '核销时间',
  `VerificationUser` varchar(50) DEFAULT NULL COMMENT '核销人',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=346 DEFAULT CHARSET=utf8 COMMENT='订单核销码表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_orderverificationcode`
--

LOCK TABLES `mall_orderverificationcode` WRITE;
/*!40000 ALTER TABLE `mall_orderverificationcode` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_orderverificationcode` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_paymentconfig`
--

DROP TABLE IF EXISTS `mall_paymentconfig`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_paymentconfig` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `IsCashOnDelivery` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_paymentconfig`
--

LOCK TABLES `mall_paymentconfig` WRITE;
/*!40000 ALTER TABLE `mall_paymentconfig` DISABLE KEYS */;
INSERT INTO `mall_paymentconfig` VALUES (1,1);
/*!40000 ALTER TABLE `mall_paymentconfig` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_pendingsettlementorder`
--

DROP TABLE IF EXISTS `mall_pendingsettlementorder`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_pendingsettlementorder` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `ShopName` varchar(100) DEFAULT NULL COMMENT '店铺名称',
  `OrderId` bigint(20) NOT NULL COMMENT '订单号',
  `OrderType` int(11) DEFAULT NULL COMMENT '订单类型',
  `OrderAmount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '订单金额',
  `ProductsAmount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '商品实付金额',
  `FreightAmount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '运费',
  `TaxAmount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '税费',
  `IntegralDiscount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '积分抵扣金额',
  `PlatCommission` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '平台佣金',
  `DistributorCommission` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '分销佣金',
  `RefundAmount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '退款金额',
  `RefundDate` datetime DEFAULT NULL COMMENT '退款时间',
  `PlatCommissionReturn` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '平台佣金退还',
  `DistributorCommissionReturn` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '分销佣金退还',
  `SettlementAmount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '结算金额',
  `OrderFinshTime` datetime DEFAULT NULL COMMENT '订单完成时间',
  `PaymentTypeName` varchar(100) DEFAULT NULL,
  `CreateDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=80 DEFAULT CHARSET=utf8 COMMENT='待结算订单表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_pendingsettlementorder`
--

LOCK TABLES `mall_pendingsettlementorder` WRITE;
/*!40000 ALTER TABLE `mall_pendingsettlementorder` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_pendingsettlementorder` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_photospace`
--

DROP TABLE IF EXISTS `mall_photospace`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_photospace` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `PhotoCategoryId` bigint(20) NOT NULL COMMENT '图片分组ID',
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `PhotoName` varchar(2000) DEFAULT NULL COMMENT '图片名称',
  `PhotoPath` varchar(2000) DEFAULT NULL COMMENT '图片路径',
  `FileSize` bigint(20) NOT NULL COMMENT '图片大小',
  `UploadTime` datetime NOT NULL COMMENT '图片上传时间',
  `LastUpdateTime` datetime NOT NULL COMMENT '图片最后更新时间',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=718 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_photospace`
--

LOCK TABLES `mall_photospace` WRITE;
/*!40000 ALTER TABLE `mall_photospace` DISABLE KEYS */;
INSERT INTO `mall_photospace` VALUES (102,1,0,'家装(图片)_02-广告图片 (2).jpg','/Storage/template/0/20170214/6362268264045115864931243.jpg',13178,'2017-02-14 15:24:00','2017-02-14 15:24:00'),(103,1,0,'家装(图片)_02-广告图片 (3).jpg','/Storage/template/0/20170214/6362268264084960871575168.jpg',54656,'2017-02-14 15:24:01','2017-02-14 15:24:01'),(104,1,0,'家装(图片)_02-广告图片 (4).jpg','/Storage/template/0/20170214/6362268264111524105559543.jpg',32165,'2017-02-14 15:24:01','2017-02-14 15:24:01'),(105,1,0,'家装(图片)_02-广告图片 (5).jpg','/Storage/template/0/20170214/6362268264142675571231653.jpg',40965,'2017-02-14 15:24:01','2017-02-14 15:24:01'),(106,1,0,'家装(图片)_02-广告图片 (6).jpg','/Storage/template/0/20170214/6362268264163866841702702.jpg',16228,'2017-02-14 15:24:02','2017-02-14 15:24:02'),(107,1,0,'家装(图片)_02-广告图片 (7).jpg','/Storage/template/0/20170214/6362268264197946038131475.jpg',44267,'2017-02-14 15:24:02','2017-02-14 15:24:02'),(108,1,0,'家装(图片)_02-广告图片 (8).jpg','/Storage/template/0/20170214/6362268264219433047129941.jpg',16153,'2017-02-14 15:24:02','2017-02-14 15:24:02'),(109,1,0,'家装(图片)_02-广告图片 (9).jpg','/Storage/template/0/20170214/6362268264251266142802051.jpg',41881,'2017-02-14 15:24:03','2017-02-14 15:24:03'),(110,1,0,'家装(图片)_02-广告图片 (10).jpg','/Storage/template/0/20170214/6362268264273142136502345.jpg',16068,'2017-02-14 15:24:03','2017-02-14 15:24:03'),(111,1,0,'家装(图片)_02-广告图片 (11).jpg','/Storage/template/0/20170214/6362268264307811062458536.jpg',50084,'2017-02-14 15:24:03','2017-02-14 15:24:03'),(112,1,0,'轮播1.jpg','/Storage/template/0/20170214/6362268264368163535344265.jpg',115249,'2017-02-14 15:24:04','2017-02-14 15:24:04'),(113,1,0,'轮播2.jpg','/Storage/template/0/20170214/6362268264410641849515609.jpg',72532,'2017-02-14 15:24:04','2017-02-14 15:24:04'),(114,1,0,'商品小图-图片导航_52.jpg','/Storage/template/0/20170214/6362268265840718179075479.jpg',17994,'2017-02-14 15:24:18','2017-02-14 15:24:18'),(115,1,0,'商品小图-图片导航_53.jpg','/Storage/template/0/20170214/6362268265862002899546528.jpg',20263,'2017-02-14 15:24:19','2017-02-14 15:24:19'),(116,1,0,'商品小图-图片导航_54.jpg','/Storage/template/0/20170214/6362268265886621086003485.jpg',20458,'2017-02-14 15:24:19','2017-02-14 15:24:19'),(117,1,0,'商品小图-图片导航_57.jpg','/Storage/template/0/20170214/6362268265904585517947116.jpg',16380,'2017-02-14 15:24:19','2017-02-14 15:24:19'),(118,1,0,'商品小图-图片导航_58.jpg','/Storage/template/0/20170214/6362268265929390811174828.jpg',22858,'2017-02-14 15:24:19','2017-02-14 15:24:19'),(119,1,0,'商品小图-图片导航_59.jpg','/Storage/template/0/20170214/6362268265954586149861030.jpg',19524,'2017-02-14 15:24:20','2017-02-14 15:24:20'),(120,1,0,'商品小图-图片导航_77.jpg','/Storage/template/0/20170214/6362268265977927294561324.jpg',18240,'2017-02-14 15:24:20','2017-02-14 15:24:20'),(121,1,0,'商品小图-图片导航_78.jpg','/Storage/template/0/20170214/6362268265998236525032373.jpg',19719,'2017-02-14 15:24:20','2017-02-14 15:24:20'),(122,1,0,'商品小图-图片导航_79.jpg','/Storage/template/0/20170214/6362268266019911064030840.jpg',17509,'2017-02-14 15:24:20','2017-02-14 15:24:20'),(123,1,0,'商品小图-图片导航_82.jpg','/Storage/template/0/20170214/6362268266043553811960379.jpg',18474,'2017-02-14 15:24:20','2017-02-14 15:24:20'),(124,1,0,'商品小图-图片导航_83.jpg','/Storage/template/0/20170214/6362268266067773127417336.jpg',21107,'2017-02-14 15:24:21','2017-02-14 15:24:21'),(125,1,0,'商品小图-图片导航_84.jpg','/Storage/template/0/20170214/6362268266089650462117630.jpg',18949,'2017-02-14 15:24:21','2017-02-14 15:24:21'),(126,1,0,'商品小图-图片导航_102.jpg','/Storage/template/0/20170214/6362268266113671054345342.jpg',22332,'2017-02-14 15:24:21','2017-02-14 15:24:21'),(127,1,0,'商品小图-图片导航_103.jpg','/Storage/template/0/20170214/6362268266135933826573054.jpg',22351,'2017-02-14 15:24:21','2017-02-14 15:24:21'),(128,1,0,'商品小图-图片导航_104.jpg','/Storage/template/0/20170214/6362268266158494481273348.jpg',24885,'2017-02-14 15:24:22','2017-02-14 15:24:22'),(129,1,0,'商品小图-图片导航_107.jpg','/Storage/template/0/20170214/6362268266183101756730305.jpg',20328,'2017-02-14 15:24:22','2017-02-14 15:24:22'),(130,1,0,'商品小图-图片导航_108.jpg','/Storage/template/0/20170214/6362268266203709877201354.jpg',19519,'2017-02-14 15:24:22','2017-02-14 15:24:22'),(131,1,0,'商品小图-图片导航_109.jpg','/Storage/template/0/20170214/6362268266231735555414974.jpg',24530,'2017-02-14 15:24:22','2017-02-14 15:24:22'),(178,1,0,'轮播1.jpg','/Storage/template/0/20170214/6362268576187693276982090.jpg',133454,'2017-02-14 16:16:02','2017-02-14 16:16:02'),(179,1,0,'轮播2.jpg','/Storage/template/0/20170214/6362268576277734352310684.jpg',156115,'2017-02-14 16:16:03','2017-02-14 16:16:03'),(180,1,0,'食品(图片)_02.jpg','/Storage/template/0/20170214/6362268576326948904224598.jpg',80529,'2017-02-14 16:16:03','2017-02-14 16:16:03'),(181,1,0,'食品(图片)_17.jpg','/Storage/template/0/20170214/6362268576348042934695647.jpg',18682,'2017-02-14 16:16:03','2017-02-14 16:16:03'),(182,1,0,'食品(图片)_35.jpg','/Storage/template/0/20170214/6362268576369819993694113.jpg',18389,'2017-02-14 16:16:04','2017-02-14 16:16:04'),(183,1,0,'食品(图片)_38.jpg','/Storage/template/0/20170214/6362268576409762009338039.jpg',54305,'2017-02-14 16:16:04','2017-02-14 16:16:04'),(184,1,0,'食品(图片)_56.jpg','/Storage/template/0/20170214/6362268576430952829809088.jpg',18526,'2017-02-14 16:16:04','2017-02-14 16:16:04'),(185,1,0,'食品(图片)_59.jpg','/Storage/template/0/20170214/6362268576478517668493757.jpg',69446,'2017-02-14 16:16:05','2017-02-14 16:16:05'),(186,1,0,'食品(图片)_16.jpg','/Storage/template/0/20170214/6362268577401460444873746.jpg',54273,'2017-02-14 16:16:14','2017-02-14 16:16:14'),(187,1,0,'食品(图片)_17.jpg','/Storage/template/0/20170214/6362268577436913196600691.jpg',36536,'2017-02-14 16:16:14','2017-02-14 16:16:14'),(188,1,0,'食品(图片)_18.jpg','/Storage/template/0/20170214/6362268577463960539516138.jpg',33405,'2017-02-14 16:16:15','2017-02-14 16:16:15'),(189,1,0,'食品(图片)_47.jpg','/Storage/template/0/20170214/6362268578339644652509631.jpg',29273,'2017-02-14 16:16:23','2017-02-14 16:16:23'),(190,1,0,'食品(图片)_48.jpg','/Storage/template/0/20170214/6362268578368645998250668.jpg',29342,'2017-02-14 16:16:24','2017-02-14 16:16:24'),(191,1,0,'食品(图片)_49.jpg','/Storage/template/0/20170214/6362268578397941192166115.jpg',30888,'2017-02-14 16:16:24','2017-02-14 16:16:24'),(192,1,0,'食品(图片)_52.jpg','/Storage/template/0/20170214/6362268578426753993608980.jpg',27768,'2017-02-14 16:16:24','2017-02-14 16:16:24'),(193,1,0,'食品(图片)_53.jpg','/Storage/template/0/20170214/6362268578452535519065937.jpg',24163,'2017-02-14 16:16:25','2017-02-14 16:16:25'),(194,1,0,'食品(图片)_54.jpg','/Storage/template/0/20170214/6362268578480658497279556.jpg',26801,'2017-02-14 16:16:25','2017-02-14 16:16:25'),(195,1,0,'食品(图片)_82.jpg','/Storage/template/0/20170214/6362268578510153508722421.jpg',26735,'2017-02-14 16:16:25','2017-02-14 16:16:25'),(196,1,0,'食品(图片)_83.jpg','/Storage/template/0/20170214/6362268578538477666936041.jpg',30258,'2017-02-14 16:16:25','2017-02-14 16:16:25'),(197,1,0,'食品(图片)_84.jpg','/Storage/template/0/20170214/6362268578568354758378906.jpg',25187,'2017-02-14 16:16:26','2017-02-14 16:16:26'),(198,1,0,'食品(图片)_87.jpg','/Storage/template/0/20170214/6362268578595606016592525.jpg',23009,'2017-02-14 16:16:26','2017-02-14 16:16:26'),(199,1,0,'食品(图片)_88.jpg','/Storage/template/0/20170214/6362268578622262926278727.jpg',27560,'2017-02-14 16:16:26','2017-02-14 16:16:26'),(200,1,0,'食品(图片)_89.jpg','/Storage/template/0/20170214/6362268578651559557721592.jpg',30706,'2017-02-14 16:16:27','2017-02-14 16:16:27'),(201,1,0,'食品(图片)_117.jpg','/Storage/template/0/20170214/6362268578679782605935212.jpg',30167,'2017-02-14 16:16:27','2017-02-14 16:16:27'),(202,1,0,'食品(图片)_118.jpg','/Storage/template/0/20170214/6362268578710547827378077.jpg',26622,'2017-02-14 16:16:27','2017-02-14 16:16:27'),(203,1,0,'食品(图片)_119.jpg','/Storage/template/0/20170214/6362268578738473445591696.jpg',28599,'2017-02-14 16:16:27','2017-02-14 16:16:27'),(204,1,0,'食品(图片)_122.jpg','/Storage/template/0/20170214/6362268578772066873020469.jpg',30284,'2017-02-14 16:16:28','2017-02-14 16:16:28'),(205,1,0,'食品(图片)_123.jpg','/Storage/template/0/20170214/6362268578803514982990751.jpg',28767,'2017-02-14 16:16:28','2017-02-14 16:16:28'),(206,1,0,'食品(图片)_124.jpg','/Storage/template/0/20170214/6362268578839646273648769.jpg',34270,'2017-02-14 16:16:28','2017-02-14 16:16:28'),(207,1,0,'食品(图片)_30.jpg','/Storage/template/0/20170214/6362268579393745886985901.jpg',72536,'2017-02-14 16:16:34','2017-02-14 16:16:34'),(208,1,0,'食品(图片)_31.jpg','/Storage/template/0/20170214/6362268579429391734414674.jpg',54338,'2017-02-14 16:16:34','2017-02-14 16:16:34'),(209,1,0,'食品(图片)_62.jpg','/Storage/template/0/20170214/6362268579469239785356772.jpg',67257,'2017-02-14 16:16:35','2017-02-14 16:16:35'),(210,1,0,'食品(图片)_63.jpg','/Storage/template/0/20170214/6362268579511322959528116.jpg',64124,'2017-02-14 16:16:35','2017-02-14 16:16:35'),(211,1,0,'食品(图片)_90.jpg','/Storage/template/0/20170214/6362268579551656796172041.jpg',60596,'2017-02-14 16:16:36','2017-02-14 16:16:36'),(212,1,0,'食品(图片)_91.jpg','/Storage/template/0/20170214/6362268579597460323100047.jpg',79533,'2017-02-14 16:16:36','2017-02-14 16:16:36'),(222,1,0,'食品(图片)_14.jpg','/Storage/template/0/20170214/6362268678382618384806848.jpg',19215,'2017-02-14 16:33:04','2017-02-14 16:33:04'),(223,1,0,'食品(图片)_15.jpg','/Storage/template/0/20170214/6362268678434469754179253.jpg',19696,'2017-02-14 16:33:04','2017-02-14 16:33:04'),(224,1,0,'食品(图片)_16.jpg','/Storage/template/0/20170214/6362268678458982586406964.jpg',20360,'2017-02-14 16:33:05','2017-02-14 16:33:05'),(225,1,0,'食品(图片)_17.jpg','/Storage/template/0/20170214/6362268678513375844306786.jpg',20174,'2017-02-14 16:33:05','2017-02-14 16:33:05'),(226,1,0,'食品(图片)_20.jpg','/Storage/template/0/20170214/6362268678538573819763743.jpg',21300,'2017-02-14 16:33:05','2017-02-14 16:33:05'),(227,1,0,'食品(图片)_21.jpg','/Storage/template/0/20170214/6362268678560744294464037.jpg',19649,'2017-02-14 16:33:06','2017-02-14 16:33:06'),(228,1,0,'食品(图片)_22.jpg','/Storage/template/0/20170214/6362268678581735363462504.jpg',21194,'2017-02-14 16:33:06','2017-02-14 16:33:06'),(229,1,0,'食品(图片)_23.jpg','/Storage/template/0/20170214/6362268678609280016377951.jpg',21924,'2017-02-14 16:33:06','2017-02-14 16:33:06'),(232,0,0,'轮播1.jpg','/Storage/template/0/20170214/6362268713516792534803144.jpg',76022,'2017-02-14 16:38:55','2017-02-14 16:38:55'),(233,0,0,'轮播2.jpg','/Storage/template/0/20170214/6362268713573927304459629.jpg',104229,'2017-02-14 16:38:56','2017-02-14 16:38:56'),(234,0,0,'轮播3.jpg','/Storage/template/0/20170214/6362268713632807357345358.jpg',111587,'2017-02-14 16:38:56','2017-02-14 16:38:56'),(235,0,0,'轮播4.jpg','/Storage/template/0/20170214/6362268713694817168758506.jpg',118513,'2017-02-14 16:38:57','2017-02-14 16:38:57'),(236,0,0,'医药(图片)_20.jpg','/Storage/template/0/20170214/6362268713716597673458800.jpg',12723,'2017-02-14 16:38:57','2017-02-14 16:38:57'),(237,0,0,'医药(图片)_32.jpg','/Storage/template/0/20170214/6362268713736423573929849.jpg',12760,'2017-02-14 16:38:57','2017-02-14 16:38:57'),(238,0,0,'医药(图片)_41.jpg','/Storage/template/0/20170214/6362268713758590857630143.jpg',12854,'2017-02-14 16:38:58','2017-02-14 16:38:58'),(239,0,0,'医药(图片)_53.jpg','/Storage/template/0/20170214/6362268713779489248101192.jpg',13111,'2017-02-14 16:38:58','2017-02-14 16:38:58'),(240,0,0,'医药(图片)_59.jpg','/Storage/template/0/20170214/6362268713799528528572241.jpg',12660,'2017-02-14 16:38:58','2017-02-14 16:38:58'),(241,0,0,'医药(图片)_65.jpg','/Storage/template/0/20170214/6362268713820409773272535.jpg',12969,'2017-02-14 16:38:58','2017-02-14 16:38:58'),(242,0,0,'医药(图片)_37.jpg','/Storage/template/0/20170214/6362268715123434859790958.jpg',15482,'2017-02-14 16:39:11','2017-02-14 16:39:11'),(243,0,0,'医药(图片)_38.jpg','/Storage/template/0/20170214/6362268715147946097720498.jpg',17127,'2017-02-14 16:39:11','2017-02-14 16:39:11'),(244,0,0,'医药(图片)_39.jpg','/Storage/template/0/20170214/6362268715172940394177455.jpg',18813,'2017-02-14 16:39:12','2017-02-14 16:39:12'),(245,0,0,'医药(图片)_42.jpg','/Storage/template/0/20170214/6362268715197558109634412.jpg',15586,'2017-02-14 16:39:12','2017-02-14 16:39:12'),(246,0,0,'医药(图片)_43.jpg','/Storage/template/0/20170214/6362268715222163436091368.jpg',17067,'2017-02-14 16:39:12','2017-02-14 16:39:12'),(247,0,0,'医药(图片)_44.jpg','/Storage/template/0/20170214/6362268715242575586562417.jpg',18543,'2017-02-14 16:39:12','2017-02-14 16:39:12'),(248,0,0,'医药(图片)_57.jpg','/Storage/template/0/20170214/6362268715263570737033467.jpg',14298,'2017-02-14 16:39:13','2017-02-14 16:39:13'),(249,0,0,'医药(图片)_58.jpg','/Storage/template/0/20170214/6362268715283007537504516.jpg',19122,'2017-02-14 16:39:13','2017-02-14 16:39:13'),(250,0,0,'医药(图片)_59.jpg','/Storage/template/0/20170214/6362268715305659132204810.jpg',17942,'2017-02-14 16:39:13','2017-02-14 16:39:13'),(251,0,0,'医药(图片)_62.jpg','/Storage/template/0/20170214/6362268715337305662175092.jpg',13913,'2017-02-14 16:39:13','2017-02-14 16:39:13'),(252,0,0,'医药(图片)_63.jpg','/Storage/template/0/20170214/6362268715358694005875386.jpg',18888,'2017-02-14 16:39:14','2017-02-14 16:39:14'),(253,0,0,'医药(图片)_64.jpg','/Storage/template/0/20170214/6362268715380665769575681.jpg',17705,'2017-02-14 16:39:14','2017-02-14 16:39:14'),(254,0,0,'医药(图片)_72.jpg','/Storage/template/0/20170214/6362268715405758336032638.jpg',18065,'2017-02-14 16:39:14','2017-02-14 16:39:14'),(255,0,0,'医药(图片)_73.jpg','/Storage/template/0/20170214/6362268715432029831017012.jpg',22077,'2017-02-14 16:39:14','2017-02-14 16:39:14'),(256,0,0,'医药(图片)_74.jpg','/Storage/template/0/20170214/6362268715454293584717306.jpg',17108,'2017-02-14 16:39:15','2017-02-14 16:39:15'),(257,0,0,'医药(图片)_77.jpg','/Storage/template/0/20170214/6362268715477145312646846.jpg',18048,'2017-02-14 16:39:15','2017-02-14 16:39:15'),(258,0,0,'医药(图片)_78.jpg','/Storage/template/0/20170214/6362268715499705551645312.jpg',22295,'2017-02-14 16:39:15','2017-02-14 16:39:15'),(259,0,0,'医药(图片)_79.jpg','/Storage/template/0/20170214/6362268715522945128574852.jpg',17244,'2017-02-14 16:39:15','2017-02-14 16:39:15'),(260,0,0,'医药(图片)_07.jpg','/Storage/template/0/20170214/6362268715910151793157684.jpg',22070,'2017-02-14 16:39:19','2017-02-14 16:39:19'),(261,0,0,'医药(图片)_08.jpg','/Storage/template/0/20170214/6362268715936622207142058.jpg',25017,'2017-02-14 16:39:19','2017-02-14 16:39:19'),(262,0,0,'医药(图片)_09.jpg','/Storage/template/0/20170214/6362268715964351111057506.jpg',30199,'2017-02-14 16:39:20','2017-02-14 16:39:20'),(263,0,0,'医药(图片)_20.jpg','/Storage/template/0/20170214/6362268716616010446465803.jpg',13077,'2017-02-14 16:39:26','2017-02-14 16:39:26'),(264,0,0,'医药(图片)_21.jpg','/Storage/template/0/20170214/6362268716636130046936852.jpg',11691,'2017-02-14 16:39:26','2017-02-14 16:39:26'),(265,0,0,'医药(图片)_22.jpg','/Storage/template/0/20170214/6362268716656153677407901.jpg',11387,'2017-02-14 16:39:27','2017-02-14 16:39:27'),(266,0,0,'医药(图片)_23.jpg','/Storage/template/0/20170214/6362268716678711262108195.jpg',12817,'2017-02-14 16:39:27','2017-02-14 16:39:27'),(267,0,0,'医药(图片)_26.jpg','/Storage/template/0/20170214/6362268716700485065808490.jpg',12784,'2017-02-14 16:39:27','2017-02-14 16:39:27'),(268,0,0,'医药(图片)_27.jpg','/Storage/template/0/20170214/6362268716720993906279539.jpg',12427,'2017-02-14 16:39:27','2017-02-14 16:39:27'),(269,0,0,'医药(图片)_28.jpg','/Storage/template/0/20170214/6362268716739844458223170.jpg',11669,'2017-02-14 16:39:27','2017-02-14 16:39:27'),(270,0,0,'医药(图片)_29.jpg','/Storage/template/0/20170214/6362268716762107957221637.jpg',13291,'2017-02-14 16:39:28','2017-02-14 16:39:28'),(275,0,0,'综合类(0)_20_01.jpg','/Storage/template/0/20170214/6362268805285934167306110.jpg',48395,'2017-02-14 16:54:13','2017-02-14 16:54:13'),(276,0,0,'综合类(0)_20_02.jpg','/Storage/template/0/20170214/6362268805329100602477453.jpg',28321,'2017-02-14 16:54:13','2017-02-14 16:54:13'),(277,0,0,'综合类(0)_20_03.jpg','/Storage/template/0/20170214/6362268805353039067934410.jpg',24578,'2017-02-14 16:54:14','2017-02-14 16:54:14'),(278,0,0,'轮播1.jpg','/Storage/template/0/20170214/6362268806031833744028910.jpg',110865,'2017-02-14 16:54:20','2017-02-14 16:54:20'),(279,0,0,'轮播2.jpg','/Storage/template/0/20170214/6362268806079786707415406.jpg',83707,'2017-02-14 16:54:21','2017-02-14 16:54:21'),(280,0,0,'轮播3.jpg','/Storage/template/0/20170214/6362268806120020595128259.jpg',59601,'2017-02-14 16:54:21','2017-02-14 16:54:21'),(281,0,0,'综合类(0)_05.jpg','/Storage/template/0/20170214/6362268806156730004313695.jpg',59372,'2017-02-14 16:54:22','2017-02-14 16:54:22'),(282,0,0,'综合类(0)_17.jpg','/Storage/template/0/20170214/6362268806185927795756560.jpg',20329,'2017-02-14 16:54:22','2017-02-14 16:54:22'),(283,0,0,'综合类(0)_20.jpg','/Storage/template/0/20170214/6362268806279680576071062.jpg',184713,'2017-02-14 16:54:23','2017-02-14 16:54:23'),(284,0,0,'综合类(0)_23.jpg','/Storage/template/0/20170214/6362268806305272762528019.jpg',21187,'2017-02-14 16:54:23','2017-02-14 16:54:23'),(285,0,0,'综合类(0)_29.jpg','/Storage/template/0/20170214/6362268806332128416512393.jpg',22157,'2017-02-14 16:54:23','2017-02-14 16:54:23'),(286,0,0,'综合类(0)_38.jpg','/Storage/template/0/20170214/6362268806355858414441933.jpg',20867,'2017-02-14 16:54:24','2017-02-14 16:54:24'),(287,0,0,'综合类(0)_47.jpg','/Storage/template/0/20170214/6362268806400096301369939.jpg',72308,'2017-02-14 16:54:24','2017-02-14 16:54:24'),(288,0,0,'综合类(0)_53.jpg','/Storage/template/0/20170214/6362268806437885739555374.jpg',60087,'2017-02-14 16:54:24','2017-02-14 16:54:24'),(289,0,0,'综合类(0)_59.jpg','/Storage/template/0/20170214/6362268806468846219525656.jpg',41467,'2017-02-14 16:54:25','2017-02-14 16:54:25'),(290,0,0,'综合类(0)_42.jpg','/Storage/template/0/20170214/6362268806818942524923053.jpg',21238,'2017-02-14 16:54:28','2017-02-14 16:54:28'),(291,0,0,'综合类(0)_43.jpg','/Storage/template/0/20170214/6362268806843160652852593.jpg',20136,'2017-02-14 16:54:28','2017-02-14 16:54:28'),(292,0,0,'综合类(0)_44.jpg','/Storage/template/0/20170214/6362268806868065698309550.jpg',18967,'2017-02-14 16:54:29','2017-02-14 16:54:29'),(293,0,0,'综合类(0)_47.jpg','/Storage/template/0/20170214/6362268806894823753293924.jpg',23573,'2017-02-14 16:54:29','2017-02-14 16:54:29'),(294,0,0,'综合类(0)_48.jpg','/Storage/template/0/20170214/6362268806918355101223463.jpg',22468,'2017-02-14 16:54:29','2017-02-14 16:54:29'),(295,0,0,'综合类(0)_49.jpg','/Storage/template/0/20170214/6362268806948820231193746.jpg',32989,'2017-02-14 16:54:29','2017-02-14 16:54:29'),(296,0,0,'综合类(0)_62.jpg','/Storage/template/0/20170214/6362268806974703796650703.jpg',22378,'2017-02-14 16:54:30','2017-02-14 16:54:30'),(297,0,0,'综合类(0)_63.jpg','/Storage/template/0/20170214/6362268806998141444580242.jpg',20830,'2017-02-14 16:54:30','2017-02-14 16:54:30'),(298,0,0,'综合类(0)_64.jpg','/Storage/template/0/20170214/6362268807022649331037199.jpg',17782,'2017-02-14 16:54:30','2017-02-14 16:54:30'),(299,0,0,'综合类(0)_67.jpg','/Storage/template/0/20170214/6362268807046475573264911.jpg',19199,'2017-02-14 16:54:30','2017-02-14 16:54:30'),(300,0,0,'综合类(0)_68.jpg','/Storage/template/0/20170214/6362268807068258516965205.jpg',23091,'2017-02-14 16:54:31','2017-02-14 16:54:31'),(301,0,0,'综合类(0)_69.jpg','/Storage/template/0/20170214/6362268807100774201164733.jpg',31605,'2017-02-14 16:54:31','2017-02-14 16:54:31'),(302,0,0,'综合类(0)_82.jpg','/Storage/template/0/20170214/6362268807123634453392444.jpg',23158,'2017-02-14 16:54:31','2017-02-14 16:54:31'),(303,0,0,'综合类(0)_83.jpg','/Storage/template/0/20170214/6362268807153904454835309.jpg',29293,'2017-02-14 16:54:32','2017-02-14 16:54:32'),(304,0,0,'综合类(0)_84.jpg','/Storage/template/0/20170214/6362268807182024753048929.jpg',30017,'2017-02-14 16:54:32','2017-02-14 16:54:32'),(305,0,0,'综合类(0)_87.jpg','/Storage/template/0/20170214/6362268807211516634491793.jpg',22779,'2017-02-14 16:54:32','2017-02-14 16:54:32'),(306,0,0,'综合类(0)_88.jpg','/Storage/template/0/20170214/6362268807248825253677229.jpg',33006,'2017-02-14 16:54:32','2017-02-14 16:54:32'),(307,0,0,'综合类(0)_89.jpg','/Storage/template/0/20170214/6362268807273629669134186.jpg',23002,'2017-02-14 16:54:33','2017-02-14 16:54:33'),(308,0,0,'综合类(0)_20.jpg','/Storage/template/0/20170214/6362268807493749048260890.jpg',16424,'2017-02-14 16:54:35','2017-02-14 16:54:35'),(309,0,0,'综合类(0)_21.jpg','/Storage/template/0/20170214/6362268807521380267947092.jpg',16587,'2017-02-14 16:54:35','2017-02-14 16:54:35'),(310,0,0,'综合类(0)_22.jpg','/Storage/template/0/20170214/6362268807544332531174804.jpg',16896,'2017-02-14 16:54:35','2017-02-14 16:54:35'),(311,0,0,'综合类(0)_23.jpg','/Storage/template/0/20170214/6362268807567379488104344.jpg',16022,'2017-02-14 16:54:36','2017-02-14 16:54:36'),(312,0,0,'综合类(0)_26.jpg','/Storage/template/0/20170214/6362268807587598258575393.jpg',16396,'2017-02-14 16:54:36','2017-02-14 16:54:36'),(313,0,0,'综合类(0)_27.jpg','/Storage/template/0/20170214/6362268807608788049046442.jpg',15667,'2017-02-14 16:54:36','2017-02-14 16:54:36'),(314,0,0,'综合类(0)_28.jpg','/Storage/template/0/20170214/6362268807630272873746736.jpg',17315,'2017-02-14 16:54:36','2017-02-14 16:54:36'),(315,0,0,'综合类(0)_29.jpg','/Storage/template/0/20170214/6362268807658003261960355.jpg',16469,'2017-02-14 16:54:37','2017-02-14 16:54:37'),(316,0,0,'图片导航1.jpg','/Storage/template/0/20170215/6362275705581767107148024.jpg',7447,'2017-02-15 12:04:16','2017-02-15 12:04:16'),(317,0,0,'图片导航2.jpg','/Storage/template/0/20170215/6362275705606472553604981.jpg',13535,'2017-02-15 12:04:16','2017-02-15 12:04:16'),(318,0,0,'图片导航3.jpg','/Storage/template/0/20170215/6362275705623951902319367.jpg',10917,'2017-02-15 12:04:16','2017-02-15 12:04:16'),(319,0,0,'图片导航4.jpg','/Storage/template/0/20170215/6362275705647290259248907.jpg',15688,'2017-02-15 12:04:16','2017-02-15 12:04:16'),(320,0,0,'轮播1.jpg','/Storage/template/0/20170215/6362275706447922602398946.jpg',31945,'2017-02-15 12:04:24','2017-02-15 12:04:24'),(321,0,0,'轮播2.jpg','/Storage/template/0/20170215/6362275706492353359799534.jpg',64458,'2017-02-15 12:04:25','2017-02-15 12:04:25'),(322,0,0,'轮播3.jpg','/Storage/template/0/20170215/6362275706527995605755724.jpg',67998,'2017-02-15 12:04:25','2017-02-15 12:04:25'),(323,0,0,'橱窗_01.jpg','/Storage/template/0/20170215/6362275707242207702508242.jpg',47706,'2017-02-15 12:04:32','2017-02-15 12:04:32'),(324,0,0,'橱窗_02.jpg','/Storage/template/0/20170215/6362275707268280256492616.jpg',19164,'2017-02-15 12:04:33','2017-02-15 12:04:33'),(325,0,0,'橱窗_04.jpg','/Storage/template/0/20170215/6362275707294548102949573.jpg',21430,'2017-02-15 12:04:33','2017-02-15 12:04:33'),(326,0,0,'1 (1).jpg','/Storage/template/0/20170215/6362275708792303807839571.jpg',44134,'2017-02-15 12:04:48','2017-02-15 12:04:48'),(327,0,0,'1 (2).jpg','/Storage/template/0/20170215/6362275708814568002539866.jpg',17398,'2017-02-15 12:04:48','2017-02-15 12:04:48'),(328,0,0,'1 (3).jpg','/Storage/template/0/20170215/6362275708835660401538332.jpg',16871,'2017-02-15 12:04:48','2017-02-15 12:04:48'),(329,0,0,'1 (4).jpg','/Storage/template/0/20170215/6362275708855971602009381.jpg',17653,'2017-02-15 12:04:49','2017-02-15 12:04:49'),(330,0,0,'1 (5).jpg','/Storage/template/0/20170215/6362275708878431105709675.jpg',21175,'2017-02-15 12:04:49','2017-02-15 12:04:49'),(331,0,0,'1 (6).jpg','/Storage/template/0/20170215/6362275708901574153639215.jpg',17368,'2017-02-15 12:04:49','2017-02-15 12:04:49'),(332,0,0,'1 (7).jpg','/Storage/template/0/20170215/6362275708924228955866927.jpg',19710,'2017-02-15 12:04:49','2017-02-15 12:04:49'),(333,0,0,'1 (8).jpg','/Storage/template/0/20170215/6362275708963093655052362.jpg',28476,'2017-02-15 12:04:50','2017-02-15 12:04:50'),(334,0,0,'1 (9).jpg','/Storage/template/0/20170215/6362275708983795458752656.jpg',17814,'2017-02-15 12:04:50','2017-02-15 12:04:50'),(335,0,0,'1 (10).jpg','/Storage/template/0/20170215/6362275709002251305994460.jpg',17646,'2017-02-15 12:04:50','2017-02-15 12:04:50'),(336,0,0,'广告图片.jpg','/Storage/template/0/20170215/6362275709042092508409141.jpg',70070,'2017-02-15 12:04:50','2017-02-15 12:04:50'),(337,0,0,'广告1.jpg','/Storage/template/0/20170215/6362275709493528456161783.jpg',71276,'2017-02-15 12:04:55','2017-02-15 12:04:55'),(338,0,0,'1 (1).jpg','/Storage/template/0/20170215/6362275709932465205887774.jpg',18691,'2017-02-15 12:04:59','2017-02-15 12:04:59'),(339,0,0,'1 (2).jpg','/Storage/template/0/20170215/6362275709951702257831406.jpg',18691,'2017-02-15 12:05:00','2017-02-15 12:05:00'),(340,0,0,'1 (3).jpg','/Storage/template/0/20170215/6362275709970255755073210.jpg',23898,'2017-02-15 12:05:00','2017-02-15 12:05:00'),(341,0,0,'1 (4).jpg','/Storage/template/0/20170215/6362275709989688105544259.jpg',23898,'2017-02-15 12:05:00','2017-02-15 12:05:00'),(342,0,0,'1 (5).jpg','/Storage/template/0/20170215/6362275710024158552973031.jpg',61104,'2017-02-15 12:05:00','2017-02-15 12:05:00'),(343,0,0,'1 (6).jpg','/Storage/template/0/20170215/6362275710069761108901037.jpg',61104,'2017-02-15 12:05:01','2017-02-15 12:05:01'),(344,0,0,'1 (7).jpg','/Storage/template/0/20170215/6362275710093685352128749.jpg',33651,'2017-02-15 12:05:01','2017-02-15 12:05:01'),(345,0,0,'1 (8).jpg','/Storage/template/0/20170215/6362275710118390807585706.jpg',33651,'2017-02-15 12:05:01','2017-02-15 12:05:01'),(346,0,0,'轮播服装类.jpg','/Storage/template/0/20170215/6362276284288861757245427.jpg',108353,'2017-02-15 13:40:43','2017-02-15 13:40:43'),(347,0,0,'轮播服装类2.jpg','/Storage/template/0/20170215/6362276284318257418688292.jpg',56115,'2017-02-15 13:40:43','2017-02-15 13:40:43'),(348,0,0,'轮播服装类3.jpg','/Storage/template/0/20170215/6362276284344625613672666.jpg',42614,'2017-02-15 13:40:43','2017-02-15 13:40:43'),(349,0,0,'轮播服装类4.jpg','/Storage/template/0/20170215/6362276284376365118344776.jpg',50856,'2017-02-15 13:40:44','2017-02-15 13:40:44'),(350,0,0,'1.jpg','/Storage/template/0/20170215/6362276307269236355174749.jpg',63148,'2017-02-15 13:44:33','2017-02-15 13:44:33'),(351,0,0,'2.jpg','/Storage/template/0/20170215/6362276307316113159630173.jpg',100121,'2017-02-15 13:44:33','2017-02-15 13:44:33'),(352,0,0,'3.jpg','/Storage/template/0/20170215/6362276307358888234801516.jpg',82861,'2017-02-15 13:44:34','2017-02-15 13:44:34'),(353,0,0,'商家图片轮播_01.jpg','/Storage/template/0/20170215/6362276313140750874573719.jpg',8127,'2017-02-15 13:45:31','2017-02-15 13:45:31'),(354,0,0,'商家图片轮播_02.jpg','/Storage/template/0/20170215/6362276313159696915044768.jpg',8594,'2017-02-15 13:45:32','2017-02-15 13:45:32'),(355,0,0,'商家图片轮播_03.jpg','/Storage/template/0/20170215/6362276313177666353759154.jpg',8632,'2017-02-15 13:45:32','2017-02-15 13:45:32'),(356,0,0,'商家橱窗_01.jpg','/Storage/template/0/20170215/6362276317863100176709333.jpg',105715,'2017-02-15 13:46:19','2017-02-15 13:46:19'),(357,0,0,'商家橱窗_02.jpg','/Storage/template/0/20170215/6362276317894156052381443.jpg',56952,'2017-02-15 13:46:19','2017-02-15 13:46:19'),(358,0,0,'商家橱窗_03.jpg','/Storage/template/0/20170215/6362276317908121435109921.jpg',1374,'2017-02-15 13:46:19','2017-02-15 13:46:19'),(359,0,0,'商家橱窗_04.jpg','/Storage/template/0/20170215/6362276317930778557337633.jpg',26643,'2017-02-15 13:46:19','2017-02-15 13:46:19'),(360,0,0,'1.jpg','/Storage/template/0/20170215/6362276322403899538903677.jpg',4906,'2017-02-15 13:47:04','2017-02-15 13:47:04'),(361,0,0,'2.jpg','/Storage/template/0/20170215/6362276322418743855861401.jpg',4869,'2017-02-15 13:47:04','2017-02-15 13:47:04'),(362,0,0,'3.jpg','/Storage/template/0/20170215/6362276322429877095360634.jpg',4847,'2017-02-15 13:47:04','2017-02-15 13:47:04'),(363,0,0,'4.jpg','/Storage/template/0/20170215/6362276322445014395547603.jpg',4861,'2017-02-15 13:47:04','2017-02-15 13:47:04'),(364,0,0,'5.jpg','/Storage/template/0/20170215/6362276322459761056803499.jpg',4960,'2017-02-15 13:47:05','2017-02-15 13:47:05'),(365,0,0,'1.jpg','/Storage/template/0/20170215/6362276326601716975915778.jpg',57325,'2017-02-15 13:47:46','2017-02-15 13:47:46'),(366,0,0,'2.jpg','/Storage/template/0/20170215/6362276326630233694129398.jpg',44294,'2017-02-15 13:47:46','2017-02-15 13:47:46'),(367,0,0,'3.jpg','/Storage/template/0/20170215/6362276326662461497328925.jpg',56411,'2017-02-15 13:47:47','2017-02-15 13:47:47'),(368,0,0,'4.jpg','/Storage/template/0/20170215/6362276326687364793785882.jpg',33651,'2017-02-15 13:47:47','2017-02-15 13:47:47'),(369,0,0,'5.jpg','/Storage/template/0/20170215/6362276326720666852687238.jpg',55198,'2017-02-15 13:47:47','2017-02-15 13:47:47'),(370,0,0,'1.jpg','/Storage/template/0/20170215/6362276353957836116569225.jpg',47991,'2017-02-15 13:52:20','2017-02-15 13:52:20'),(371,0,0,'2.jpg','/Storage/template/0/20170215/6362276353999923261740569.jpg',49599,'2017-02-15 13:52:20','2017-02-15 13:52:20'),(372,0,0,'3.jpg','/Storage/template/0/20170215/6362276354023163963968280.jpg',23941,'2017-02-15 13:52:20','2017-02-15 13:52:20'),(373,0,0,'图片导航_01.jpg','/Storage/template/0/20170215/6362276361315310146657132.jpg',4156,'2017-02-15 13:53:33','2017-02-15 13:53:33'),(374,0,0,'图片导航_02.jpg','/Storage/template/0/20170215/6362276361328884883614856.jpg',3889,'2017-02-15 13:53:33','2017-02-15 13:53:33'),(375,0,0,'图片导航_03.jpg','/Storage/template/0/20170215/6362276361341190046343334.jpg',4240,'2017-02-15 13:53:33','2017-02-15 13:53:33'),(376,0,0,'图片导航_04.jpg','/Storage/template/0/20170215/6362276361354960109071813.jpg',4425,'2017-02-15 13:53:34','2017-02-15 13:53:34'),(377,0,0,'图片导航_05.jpg','/Storage/template/0/20170215/6362276361366386322800291.jpg',4558,'2017-02-15 13:53:34','2017-02-15 13:53:34'),(378,0,0,'图片导航_06.jpg','/Storage/template/0/20170215/6362276361382402567285432.jpg',3930,'2017-02-15 13:53:34','2017-02-15 13:53:34'),(379,0,0,'图片导航_07.jpg','/Storage/template/0/20170215/6362276361393633462486493.jpg',4320,'2017-02-15 13:53:34','2017-02-15 13:53:34'),(380,0,0,'图片导航_08.jpg','/Storage/template/0/20170215/6362276361404083081985727.jpg',4522,'2017-02-15 13:53:34','2017-02-15 13:53:34'),(381,0,0,'1.jpg','/Storage/template/0/20170215/6362276369400581546226035.jpg',42926,'2017-02-15 13:54:54','2017-02-15 13:54:54'),(382,0,0,'2.jpg','/Storage/template/0/20170215/6362276369426754425912237.jpg',31703,'2017-02-15 13:54:54','2017-02-15 13:54:54'),(383,0,0,'3.jpg','/Storage/template/0/20170215/6362276369455368804125857.jpg',40989,'2017-02-15 13:54:55','2017-02-15 13:54:55'),(384,0,0,'4.jpg','/Storage/template/0/20170215/6362276369480955723812059.jpg',42390,'2017-02-15 13:54:55','2017-02-15 13:54:55'),(385,0,0,'5.jpg','/Storage/template/0/20170215/6362276369509374782025678.jpg',47176,'2017-02-15 13:54:55','2017-02-15 13:54:55'),(386,0,0,'6.jpg','/Storage/template/0/20170215/6362276369540528321995961.jpg',43962,'2017-02-15 13:54:55','2017-02-15 13:54:55'),(387,0,0,'7.jpg','/Storage/template/0/20170215/6362276369568654404911408.jpg',45615,'2017-02-15 13:54:56','2017-02-15 13:54:56'),(388,0,0,'8.jpg','/Storage/template/0/20170215/6362276369597171123125028.jpg',45437,'2017-02-15 13:54:56','2017-02-15 13:54:56'),(389,0,0,'1.jpg','/Storage/template/0/20170215/6362276394718448241380830.jpg',42041,'2017-02-15 13:59:07','2017-02-15 13:59:07'),(390,0,0,'2.jpg','/Storage/template/0/20170215/6362276394742374943608541.jpg',36982,'2017-02-15 13:59:07','2017-02-15 13:59:07'),(391,0,0,'3.jpg','/Storage/template/0/20170215/6362276394769231446523989.jpg',28364,'2017-02-15 13:59:08','2017-02-15 13:59:08'),(392,0,0,'4.jpg','/Storage/template/0/20170215/6362276394787591529536547.jpg',27528,'2017-02-15 13:59:08','2017-02-15 13:59:08'),(393,0,0,'轮播电器.jpg','/Storage/template/0/20170215/6362276403887745646868868.jpg',26519,'2017-02-15 14:00:39','2017-02-15 14:00:39'),(394,0,0,'轮播电器2.jpg','/Storage/template/0/20170215/6362276403946634629754598.jpg',76250,'2017-02-15 14:00:39','2017-02-15 14:00:39'),(395,0,0,'轮播电器3.jpg','/Storage/template/0/20170215/6362276403974174747968218.jpg',46082,'2017-02-15 14:00:40','2017-02-15 14:00:40'),(396,0,0,'广告图片_01.jpg','/Storage/template/0/20170215/6362276407161894801730226.jpg',8565,'2017-02-15 14:01:12','2017-02-15 14:01:12'),(397,0,0,'广告图片_02.jpg','/Storage/template/0/20170215/6362276407176641467687949.jpg',9868,'2017-02-15 14:01:12','2017-02-15 14:01:12'),(398,0,0,'橱窗（3）_01.jpg','/Storage/template/0/20170215/6362276409524192545885238.jpg',9634,'2017-02-15 14:01:35','2017-02-15 14:01:35'),(399,0,0,'橱窗（3）_02.jpg','/Storage/template/0/20170215/6362276409542552623127041.jpg',9860,'2017-02-15 14:01:35','2017-02-15 14:01:35'),(400,0,0,'橱窗（3）_03.jpg','/Storage/template/0/20170215/6362276409560815045070673.jpg',7739,'2017-02-15 14:01:36','2017-02-15 14:01:36'),(401,0,0,'1.jpg','/Storage/template/0/20170215/6362276412808986649245828.jpg',26285,'2017-02-15 14:02:08','2017-02-15 14:02:08'),(402,0,0,'2.jpg','/Storage/template/0/20170215/6362276412835647828932030.jpg',49292,'2017-02-15 14:02:08','2017-02-15 14:02:08'),(403,0,0,'3.jpg','/Storage/template/0/20170215/6362276412862016023916405.jpg',43075,'2017-02-15 14:02:09','2017-02-15 14:02:09'),(404,0,0,'4.jpg','/Storage/template/0/20170215/6362276412884966127616699.jpg',39040,'2017-02-15 14:02:09','2017-02-15 14:02:09'),(405,0,0,'5.jpg','/Storage/template/0/20170215/6362276412904205148087748.jpg',22479,'2017-02-15 14:02:09','2017-02-15 14:02:09'),(406,0,0,'6.jpg','/Storage/template/0/20170215/6362276412929206104544705.jpg',41816,'2017-02-15 14:02:09','2017-02-15 14:02:09'),(407,0,0,'7.jpg','/Storage/template/0/20170215/6362276412962996467744232.jpg',56431,'2017-02-15 14:02:10','2017-02-15 14:02:10'),(408,0,0,'8.jpg','/Storage/template/0/20170215/6362276412994540643416342.jpg',51799,'2017-02-15 14:02:10','2017-02-15 14:02:10'),(409,0,0,'9.jpg','/Storage/template/0/20170215/6362276413023057364859207.jpg',32733,'2017-02-15 14:02:10','2017-02-15 14:02:10'),(410,0,0,'12.jpg','/Storage/template/0/20170215/6362276413062121362572060.jpg',45275,'2017-02-15 14:02:11','2017-02-15 14:02:11'),(501,0,1,'轮播1.jpg','/Storage/template/1/20170216/6362285816953645384819427.jpg',110865,'2017-02-16 16:09:30','2017-02-16 16:09:30'),(502,0,1,'轮播2.jpg','/Storage/template/1/20170216/6362285816996811108990770.jpg',83707,'2017-02-16 16:09:30','2017-02-16 16:09:30'),(503,0,1,'轮播3.jpg','/Storage/template/1/20170216/6362285817030699126419543.jpg',59601,'2017-02-16 16:09:30','2017-02-16 16:09:30'),(504,0,1,'综合类(0)_05.jpg','/Storage/template/1/20170216/6362285817065173103848316.jpg',59372,'2017-02-16 16:09:31','2017-02-16 16:09:31'),(505,0,1,'综合类(0)_17.jpg','/Storage/template/1/20170216/6362285817084509781090120.jpg',20329,'2017-02-16 16:09:31','2017-02-16 16:09:31'),(506,0,1,'综合类(0)_20.jpg','/Storage/template/1/20170216/6362285817163907364446898.jpg',184713,'2017-02-16 16:09:32','2017-02-16 16:09:32'),(507,0,1,'综合类(0)_23.jpg','/Storage/template/1/20170216/6362285817181388503161285.jpg',21187,'2017-02-16 16:09:32','2017-02-16 16:09:32'),(508,0,1,'综合类(0)_29.jpg','/Storage/template/1/20170216/6362285817201994766861579.jpg',22157,'2017-02-16 16:09:32','2017-02-16 16:09:32'),(509,0,1,'综合类(0)_38.jpg','/Storage/template/1/20170216/6362285817235882781061106.jpg',20867,'2017-02-16 16:09:32','2017-02-16 16:09:32'),(510,0,1,'综合类(0)_47.jpg','/Storage/template/1/20170216/6362285817278853183759867.jpg',72308,'2017-02-16 16:09:33','2017-02-16 16:09:33'),(511,0,1,'综合类(0)_53.jpg','/Storage/template/1/20170216/6362285817311569286959395.jpg',60087,'2017-02-16 16:09:33','2017-02-16 16:09:33'),(512,0,1,'综合类(0)_59.jpg','/Storage/template/1/20170216/6362285817339207066645597.jpg',41467,'2017-02-16 16:09:33','2017-02-16 16:09:33'),(513,0,1,'综合类(0)_20.jpg','/Storage/template/1/20170216/6362285828862110468073494.jpg',16424,'2017-02-16 16:11:29','2017-02-16 16:11:29'),(514,0,1,'综合类(0)_21.jpg','/Storage/template/1/20170216/6362285828897365725502267.jpg',16587,'2017-02-16 16:11:29','2017-02-16 16:11:29'),(515,0,1,'综合类(0)_22.jpg','/Storage/template/1/20170216/6362285828913479624216653.jpg',16896,'2017-02-16 16:11:29','2017-02-16 16:11:29'),(516,0,1,'综合类(0)_23.jpg','/Storage/template/1/20170216/6362285828931351402931040.jpg',16022,'2017-02-16 16:11:29','2017-02-16 16:11:29'),(517,0,1,'综合类(0)_26.jpg','/Storage/template/1/20170216/6362285828952543626631334.jpg',16396,'2017-02-16 16:11:30','2017-02-16 16:11:30'),(518,0,1,'综合类(0)_27.jpg','/Storage/template/1/20170216/6362285829001471285316003.jpg',15667,'2017-02-16 16:11:30','2017-02-16 16:11:30'),(519,0,1,'综合类(0)_28.jpg','/Storage/template/1/20170216/6362285829022956489016297.jpg',17315,'2017-02-16 16:11:30','2017-02-16 16:11:30'),(520,0,1,'综合类(0)_29.jpg','/Storage/template/1/20170216/6362285829044539343716591.jpg',16469,'2017-02-16 16:11:30','2017-02-16 16:11:30'),(521,0,1,'综合类(0)_42.jpg','/Storage/template/1/20170216/6362285854858030544024617.jpg',21238,'2017-02-16 16:15:49','2017-02-16 16:15:49'),(522,0,1,'综合类(0)_43.jpg','/Storage/template/1/20170216/6362285854876292967037175.jpg',20136,'2017-02-16 16:15:49','2017-02-16 16:15:49'),(523,0,1,'综合类(0)_44.jpg','/Storage/template/1/20170216/6362285854897094541737469.jpg',18967,'2017-02-16 16:15:49','2017-02-16 16:15:49'),(524,0,1,'综合类(0)_47.jpg','/Storage/template/1/20170216/6362285854919556345437764.jpg',23573,'2017-02-16 16:15:49','2017-02-16 16:15:49'),(525,0,1,'综合类(0)_48.jpg','/Storage/template/1/20170216/6362285854939186005908813.jpg',22468,'2017-02-16 16:15:49','2017-02-16 16:15:49'),(526,0,1,'综合类(0)_49.jpg','/Storage/template/1/20170216/6362285854964089308136524.jpg',32989,'2017-02-16 16:15:50','2017-02-16 16:15:50'),(527,0,1,'综合类(0)_62.jpg','/Storage/template/1/20170216/6362285854986160466066064.jpg',22378,'2017-02-16 16:15:50','2017-02-16 16:15:50'),(528,0,1,'综合类(0)_63.jpg','/Storage/template/1/20170216/6362285855005692463307868.jpg',20830,'2017-02-16 16:15:50','2017-02-16 16:15:50'),(529,0,1,'综合类(0)_64.jpg','/Storage/template/1/20170216/6362285855025224463778917.jpg',17782,'2017-02-16 16:15:50','2017-02-16 16:15:50'),(530,0,1,'综合类(0)_67.jpg','/Storage/template/1/20170216/6362285855047100307479211.jpg',19199,'2017-02-16 16:15:50','2017-02-16 16:15:50'),(531,0,1,'综合类(0)_68.jpg','/Storage/template/1/20170216/6362285855068878482179505.jpg',23091,'2017-02-16 16:15:51','2017-02-16 16:15:51'),(532,0,1,'综合类(0)_69.jpg','/Storage/template/1/20170216/6362285855093391144407217.jpg',31605,'2017-02-16 16:15:51','2017-02-16 16:15:51'),(533,0,1,'综合类(0)_82.jpg','/Storage/template/1/20170216/6362285855115657628107511.jpg',23158,'2017-02-16 16:15:51','2017-02-16 16:15:51'),(534,0,1,'综合类(0)_83.jpg','/Storage/template/1/20170216/6362285855138119421335223.jpg',29293,'2017-02-16 16:15:51','2017-02-16 16:15:51'),(535,0,1,'综合类(0)_84.jpg','/Storage/template/1/20170216/6362285855176109169520658.jpg',30017,'2017-02-16 16:15:52','2017-02-16 16:15:52'),(536,0,1,'综合类(0)_87.jpg','/Storage/template/1/20170216/6362285855197985004220953.jpg',22779,'2017-02-16 16:15:52','2017-02-16 16:15:52'),(537,0,1,'综合类(0)_88.jpg','/Storage/template/1/20170216/6362285855221325742150492.jpg',33006,'2017-02-16 16:15:52','2017-02-16 16:15:52'),(538,0,1,'综合类(0)_89.jpg','/Storage/template/1/20170216/6362285855238806885163051.jpg',23002,'2017-02-16 16:15:52','2017-02-16 16:15:52');
/*!40000 ALTER TABLE `mall_photospace` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_photospacecategory`
--

DROP TABLE IF EXISTS `mall_photospacecategory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_photospacecategory` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `PhotoSpaceCatrgoryName` varchar(255) DEFAULT NULL COMMENT '图片空间分类名称',
  `DisplaySequence` bigint(20) NOT NULL COMMENT '显示顺序',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_photospacecategory`
--

LOCK TABLES `mall_photospacecategory` WRITE;
/*!40000 ALTER TABLE `mall_photospacecategory` DISABLE KEYS */;
INSERT INTO `mall_photospacecategory` VALUES (3,0,'新建文件夹',1);
/*!40000 ALTER TABLE `mall_photospacecategory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_plataccount`
--

DROP TABLE IF EXISTS `mall_plataccount`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_plataccount` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `Balance` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '帐户余额',
  `PendingSettlement` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '待结算',
  `Settled` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '已结算',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8 COMMENT='平台资金表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_plataccount`
--

LOCK TABLES `mall_plataccount` WRITE;
/*!40000 ALTER TABLE `mall_plataccount` DISABLE KEYS */;
INSERT INTO `mall_plataccount` VALUES (1,0.00,0.00,592.20);
/*!40000 ALTER TABLE `mall_plataccount` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_plataccountitem`
--

DROP TABLE IF EXISTS `mall_plataccountitem`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_plataccountitem` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `AccountNo` varchar(50) NOT NULL COMMENT '交易流水号',
  `AccoutID` bigint(20) NOT NULL COMMENT '关联资金编号',
  `CreateTime` datetime NOT NULL COMMENT '创建时间',
  `Amount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '金额',
  `Balance` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '帐户剩余',
  `TradeType` int(4) NOT NULL DEFAULT '0' COMMENT '交易类型',
  `IsIncome` bit(1) NOT NULL COMMENT '是否收入',
  `ReMark` varchar(1000) DEFAULT NULL COMMENT '交易备注',
  `DetailId` varchar(100) DEFAULT NULL COMMENT '详情ID',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_mall_Pltem_AccoutID` (`AccoutID`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=667 DEFAULT CHARSET=utf8 COMMENT='平台资金流水表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_plataccountitem`
--

LOCK TABLES `mall_plataccountitem` WRITE;
/*!40000 ALTER TABLE `mall_plataccountitem` DISABLE KEYS */;
INSERT INTO `mall_plataccountitem` VALUES (149,'11017696',1,'2017-02-14 11:21:28',0.00,0.00,2,'','店铺购买拼团服务,12个月','101'),(150,'11024338',1,'2017-02-14 11:21:37',0.00,0.00,2,'','店铺购买拼团服务,12个月','102'),(151,'11038676',1,'2017-02-14 11:22:31',0.00,0.00,2,'','店铺购买随机红包服务,12个月','103'),(152,'11049583',1,'2017-02-14 11:35:34',0.00,0.00,2,'','店铺购买限时购服务,1个月','104'),(153,'11054374',1,'2017-02-14 11:38:30',0.00,0.00,2,'','店铺购买优惠券服务,12个月','105'),(154,'11061384',1,'2017-02-14 11:55:57',0.00,0.00,2,'','店铺购买组合购服务,12个月','106'),(155,'11078156',1,'2017-02-14 11:56:02',0.00,0.00,2,'','店铺购买组合购服务,12个月','107'),(156,'201702150050002533365',1,'2017-02-15 00:50:00',0.00,0.00,1,'','2017/2/15 0:50:00平台结算157','157'),(157,'201702160050000972607',1,'2017-02-16 00:50:00',0.00,0.00,1,'','2017/2/16 0:50:00平台结算158','158'),(158,'3081088508',1,'2017-02-16 13:50:59',0.00,0.00,2,'','店铺购买优惠券服务,12个月','108'),(159,'3091094392',1,'2017-02-16 13:56:50',0.00,0.00,2,'','店铺购买限时购服务,12个月','109'),(160,'3091102813',1,'2017-02-16 13:57:01',0.00,0.00,2,'','店铺购买优惠券服务,12个月','110'),(161,'201702170050002579063',1,'2017-02-17 00:50:00',0.00,0.00,1,'','2017/2/17 0:50:00平台结算159','159'),(162,'201702180050003084478',1,'2017-02-18 00:50:00',0.00,0.00,1,'','2017/2/18 0:50:00平台结算160','160'),(163,'201702190050001517734',1,'2017-02-19 00:50:00',0.00,0.00,1,'','2017/2/19 0:50:00平台结算161','161'),(164,'201702200950009006062',1,'2017-02-20 09:50:01',0.00,0.00,1,'','2017/2/20 9:50:00平台结算162','162');
/*!40000 ALTER TABLE `mall_plataccountitem` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_platvisit`
--

DROP TABLE IF EXISTS `mall_platvisit`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_platvisit` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '主键',
  `Date` datetime NOT NULL COMMENT '统计日期',
  `VisitCounts` bigint(20) NOT NULL COMMENT '平台浏览数',
  `OrderUserCount` bigint(20) NOT NULL COMMENT '下单人数',
  `OrderCount` bigint(20) NOT NULL COMMENT '订单数',
  `OrderProductCount` bigint(20) NOT NULL COMMENT '下单件数',
  `OrderAmount` decimal(18,2) NOT NULL COMMENT '下单金额',
  `OrderPayUserCount` bigint(20) NOT NULL COMMENT '下单付款人数',
  `OrderPayCount` bigint(20) NOT NULL COMMENT '付款订单数',
  `SaleCounts` bigint(20) NOT NULL COMMENT '付款下单件数',
  `SaleAmounts` decimal(18,2) NOT NULL COMMENT '付款金额',
  `StatisticFlag` bit(1) NOT NULL COMMENT '是否已经统计(0：未统计,1已统计)',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=5378 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_platvisit`
--

LOCK TABLES `mall_platvisit` WRITE;
/*!40000 ALTER TABLE `mall_platvisit` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_platvisit` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_product`
--

DROP TABLE IF EXISTS `mall_product`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_product` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `CategoryId` bigint(20) NOT NULL COMMENT '分类ID',
  `CategoryPath` varchar(100) NOT NULL COMMENT '分类路径',
  `ProductType` tinyint(4) NOT NULL COMMENT '商品类型(0=实物商品，1=虚拟商品)',
  `TypeId` bigint(20) NOT NULL COMMENT '类型ID',
  `BrandId` bigint(20) NOT NULL COMMENT '品牌ID',
  `ProductName` varchar(100) NOT NULL COMMENT '商品名称',
  `ProductCode` varchar(100) DEFAULT NULL COMMENT '商品编号',
  `ShortDescription` varchar(4000) DEFAULT NULL COMMENT '广告词',
  `SaleStatus` int(11) NOT NULL COMMENT '销售状态',
  `AuditStatus` int(11) NOT NULL COMMENT '审核状态',
  `AddedDate` datetime NOT NULL COMMENT '添加日期',
  `DisplaySequence` bigint(20) NOT NULL COMMENT '显示顺序',
  `ImagePath` varchar(100) DEFAULT NULL COMMENT '存放图片的目录',
  `MarketPrice` decimal(18,2) NOT NULL COMMENT '市场价',
  `MinSalePrice` decimal(18,2) NOT NULL COMMENT '最小销售价',
  `HasSKU` tinyint(1) NOT NULL COMMENT '是否有SKU',
  `VistiCounts` bigint(20) NOT NULL COMMENT '浏览次数',
  `SaleCounts` bigint(20) NOT NULL COMMENT '销售量',
  `FreightTemplateId` bigint(20) NOT NULL COMMENT '运费模板ID',
  `Weight` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '重量',
  `Volume` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '体积',
  `Quantity` int(11) NOT NULL DEFAULT '0' COMMENT '数量',
  `MeasureUnit` varchar(20) DEFAULT NULL COMMENT '计量单位',
  `EditStatus` int(11) NOT NULL DEFAULT '0' COMMENT '修改状态 0 正常 1已修改 2待审核 3 已修改并待审核',
  `IsDeleted` bit(1) NOT NULL COMMENT '是否已删除',
  `MaxBuyCount` int(11) NOT NULL COMMENT '最大购买数',
  `IsOpenLadder` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否开启阶梯价格',
  `ColorAlias` varchar(50) DEFAULT NULL COMMENT '颜色别名',
  `SizeAlias` varchar(50) DEFAULT NULL COMMENT '尺码别名',
  `VersionAlias` varchar(50) DEFAULT NULL COMMENT '版本别名',
  `ShopDisplaySequence` int(11) NOT NULL DEFAULT '0' COMMENT '商家商品序号',
  `VirtualSaleCounts` bigint(20) NOT NULL DEFAULT '0' COMMENT '虚拟销量',
  `VideoPath` varchar(200) DEFAULT NULL COMMENT '商品主图视频',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_SHOPID` (`ShopId`) USING BTREE,
  KEY `FK_CategoryId` (`CategoryId`) USING BTREE,
  KEY `IX_SaleStatus` (`SaleStatus`) USING BTREE,
  KEY `IX_AuditStatus` (`AuditStatus`) USING BTREE,
  KEY `IX_ShopId` (`ShopId`) USING BTREE,
  KEY `IX_IsDeleted` (`IsDeleted`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=1765 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_product`
--

LOCK TABLES `mall_product` WRITE;
/*!40000 ALTER TABLE `mall_product` DISABLE KEYS */;
INSERT INTO `mall_product` VALUES (699,1,3,'1|2|3',0,82,319,'三只松鼠_开口松子218gx2袋零食坚果特产炒货东北红松子原味','sz001',NULL,1,2,'2017-02-13 17:32:45',1,'/Storage/Shop/1/Products/699',59.90,49.90,0,0,1,166,0.00,0.00,0,'包',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(700,1,7,'1|2|7',0,82,319,'三只松鼠_坚果组合613g零食坚果特产碧根果夏威夷果开口松子原味','bgg001',NULL,1,2,'2017-02-13 17:36:10',1,'/Storage/Shop/1/Products/700',69.90,59.90,0,0,12,166,0.00,0.00,0,'包',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(701,1,6,'1|2|6',0,82,319,'三只松鼠 坚果炒货 休闲零食 纸皮核桃210g/袋','ht001',NULL,1,2,'2017-02-13 17:39:24',1,'/Storage/Shop/1/Products/701',39.90,29.90,0,0,7,166,0.00,0.00,0,'包',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(702,1,5,'1|4|5',0,82,320,'卫龙 休闲零食 辣条 小面筋 办公室休闲食品 22g*20包(新老包装随机发货)','wl001',NULL,1,2,'2017-02-13 17:42:06',1,'/Storage/Shop/1/Products/702',13.00,11.90,0,0,3,166,0.00,0.00,0,'包',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(703,1,5,'1|4|5',0,82,320,'卫龙 休闲零食 亲嘴条 辣条 400g/袋','wl001',NULL,2,1,'2017-02-13 17:48:53',1,'/Storage/Shop/1/Products/703',13.00,10.80,0,0,2,166,0.00,0.00,0,'包',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(704,1,13,'8|9|13',0,83,321,'ONLY2017早春新品宽松猫咪图案假两件针织衫女L|117124507 G43花灰 170/88A/L','001',NULL,1,2,'2017-02-13 17:53:09',1,'/Storage/Shop/1/Products/704',599.00,499.00,0,0,1,166,0.00,0.00,0,'件',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(705,1,10,'8|9|10',0,83,321,'ONLY2017早春新品大V领宽松下摆开衩长袖针织衫女ES|117124502 F17庆典红 165/84A/M','only001',NULL,1,2,'2017-02-13 17:55:40',1,'/Storage/Shop/1/Products/705',499.00,399.00,0,0,6,166,0.00,0.00,0,'件',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(706,1,10,'8|9|10',0,83,321,'ONLY2016早春新品拼色宽松太空棉卫衣女E|11639R511 07B酒红色 165/84A/M','only002',NULL,1,2,'2017-02-13 17:58:44',1,'/Storage/Shop/1/Products/706',300.00,234.00,0,0,0,166,0.00,0.00,0,'件',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(707,1,11,'8|9|11',0,83,321,'ONLY春季新品含莱卡面料五分袖修身露肩一字领针织连衣裙女E|116361504 010黑 175/92A/XL','only003',NULL,1,2,'2017-02-13 18:01:30',1,'/Storage/Shop/1/Products/707',200.00,175.00,1,0,4,166,0.00,0.00,0,'件',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(708,1,11,'8|9|11',0,83,321,'ONLY春季新品纯棉修身高腰镂空蕾丝连衣裙女L|116307501 021奶白 155/76A/XS','only004',NULL,1,2,'2017-02-13 18:03:48',1,'/Storage/Shop/1/Products/708',500.00,450.00,0,0,1,166,0.00,0.00,0,'件',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(709,1,12,'8|9|12',0,83,321,'ONLY2016春季新品T恤衬衫叠搭纯棉两件套连衣裙女T|116360504 10D炭花灰 165/84A/M','only006',NULL,1,4,'2017-02-13 18:06:19',1,'/Storage/Shop/1/Products/709',699.00,599.00,0,0,0,166,0.00,0.00,0,'件',4,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(710,1,11,'8|9|11',0,83,321,'ONLY2016早春新品纯棉宽松徽章贴布牛仔背带裙女T|116342524 390洗水牛仔蓝(928) 155/76A/XS','only007',NULL,1,4,'2017-02-13 18:11:32',1,'/Storage/Shop/1/Products/710',300.00,250.00,0,0,0,166,0.00,0.00,0,'件',4,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(711,1,11,'8|9|11',0,83,0,'FOREVER21 甜美花朵抽褶吊带连衣裙 黑色/铁锈色 S','zara001',NULL,2,1,'2017-02-14 09:31:14',1,'/Storage/Shop/1/Products/711',169.00,169.00,0,0,0,166,0.00,0.00,0,'件',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(712,1,11,'8|9|11',0,83,0,'[Forever21 Contemporary]纯色优雅及膝短袖连衣裙 香草色 S','zara002',NULL,2,1,'2017-02-14 09:35:30',1,'/Storage/Shop/1/Products/712',300.00,229.00,1,0,0,166,0.00,0.00,0,'件',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(713,1,13,'8|9|13',0,83,0,'XX FOREVER21 短款撞色条纹无袖针织衫 红色/白色 M','zara003',NULL,1,4,'2017-02-14 09:39:07',1,'/Storage/Shop/1/Products/713',60.00,50.00,0,0,0,166,0.00,0.00,0,'件',4,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(714,1,16,'14|15|16',0,84,322,'Apple MacBook Air 13.3英寸笔记本电脑 银色(Core i5 处理器/8GB内存/128GB SSD闪存 MMGF2CH/A)','MacBook001','成交价6288元】浪漫之选！神券满6000减600元！超强的续航能力，纤薄、轻巧、耐用！',1,4,'2017-02-14 10:14:59',1,'/Storage/Shop/1/Products/714',6988.00,6988.00,0,0,0,166,0.00,0.00,0,'台',4,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(715,1,16,'14|15|16',0,84,322,'Apple MacBook Pro 15.4英寸笔记本电脑 银色(Core i7 处理器/16GB内存/256GB SSD闪存/Retina屏 MJLQ2CH/A)','MacBook002','持久的电池使用时间，配备绚丽夺目的Retina显示屏！',2,1,'2017-02-14 10:19:19',1,'/Storage/Shop/1/Products/715',13688.00,13688.00,0,0,0,166,0.00,0.00,0,'台',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(716,1,19,'17|18|19',0,85,323,'宇食俱进厄瓜多尔白虾大虾南美活冻海鲜水产海虾 1700克40-50白虾','sx001','全场满199送橄榄皂 赠品不叠加 顺丰发出',1,2,'2017-02-14 10:31:33',1,'/Storage/Shop/1/Products/716',200.00,158.00,0,0,0,166,0.00,0.00,0,'斤',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(717,1,22,'20|21|22',0,87,324,'欧莱雅水能润泽双效洁面膏100+50ml','oly001',NULL,1,4,'2017-02-14 10:42:13',1,'/Storage/Shop/1/Products/717',25.00,19.90,0,0,0,166,0.00,0.00,0,'瓶',4,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(718,1,23,'20|21|23',0,87,328,'贝德玛（Bioderma）深层舒妍卸妆水 舒缓保湿粉水（干性中性敏感肌法国版/海外版随机发）500ml ','bdm001','正品保障！法国原产地采购，海关保税区监管发货，正品价更优！',2,1,'2017-02-14 10:47:38',1,'/Storage/Shop/1/Products/718',120.00,110.00,0,0,0,166,0.00,0.00,0,'瓶',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(719,1,22,'20|21|22',0,87,0,'资生堂洗颜专科柔澈泡沫洁面乳１２０ｇ','jm001',NULL,1,2,'2017-02-14 14:10:54',1,'/Storage/Shop/1/Products/719',40.00,37.00,0,0,0,167,0.00,0.00,0,'支',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(720,1,22,'20|21|22',0,87,324,'欧莱雅（LOREAL）男士火山岩控油清痘洁面膏100ml','jm002',NULL,1,2,'2017-02-14 14:13:38',1,'/Storage/Shop/1/Products/720',40.00,32.90,0,0,0,168,0.00,0.00,0,'支',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(721,1,22,'20|21|22',0,87,0,'丝塔芙(Cetaphil)洁面乳118ml','jm003',NULL,1,2,'2017-02-14 14:16:29',1,'/Storage/Shop/1/Products/721',60.00,55.00,0,0,0,169,0.00,0.00,0,'支',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(722,1,22,'20|21|22',0,87,0,'Clinique倩碧液体洁面皂温和型200ml','jm004',NULL,1,2,'2017-02-14 14:19:01',1,'/Storage/Shop/1/Products/722',150.00,110.00,0,0,0,168,0.00,0.00,0,'瓶',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(723,1,22,'20|21|22',0,87,0,'露得清深层净化洗面乳2支装100g*2','jm005',NULL,1,2,'2017-02-14 14:20:57',1,'/Storage/Shop/1/Products/723',60.00,50.00,0,0,0,166,0.00,0.00,0,'支',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(724,1,10,'8|9|10',0,83,0,'2016秋冬新款女装中长款羊毛大衣修身显瘦风衣西装格子毛呢外套女','wt001',NULL,1,3,'2017-02-14 14:26:18',1,'/Storage/Shop/1/Products/724',300.00,248.00,0,0,0,167,0.00,0.00,0,'件',4,'',0,'\0',NULL,NULL,NULL,0,0,NULL),(725,1,10,'8|9|10',0,83,0,'2016秋冬新款女装中长款羊毛大衣修身显瘦风衣西装格子毛呢外套女','wt001',NULL,3,1,'2017-02-14 14:28:50',1,'/Storage/Shop/1/Products/725',300.00,248.00,0,0,0,167,0.00,0.00,0,'件',3,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(726,1,10,'8|9|10',0,83,0,'佐露丝女装2017新款韩版麂皮毛毛绒外套女中长款秋冬保暖加绒大衣宽松风衣潮 浅灰 S','wt002',NULL,3,1,'2017-02-14 14:32:42',1,'/Storage/Shop/1/Products/726',400.00,388.00,0,0,0,168,0.00,0.00,0,'件',3,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(727,1,10,'8|9|10',0,83,0,'秀族 2016秋装新款韩版毛呢外套女装宽松粉色大衣中长款','wt003',NULL,3,1,'2017-02-14 14:34:49',1,'/Storage/Shop/1/Products/727',118.00,98.00,0,0,0,168,0.00,0.00,0,'件',3,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(728,1,10,'8|9|10',0,83,0,'米兰茵韩版茧型加棉羊毛呢子外套中长款宽松加厚羊绒大衣女装','wt004',NULL,3,1,'2017-02-14 14:37:11',1,'/Storage/Shop/1/Products/728',200.00,189.00,0,0,0,167,0.00,0.00,0,'件',3,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(729,1,10,'8|9|10',0,83,0,'2017秋冬加厚韩国韩范宽松中长款毛呢外套显瘦纯色大翻领','wt005',NULL,3,1,'2017-02-14 14:41:53',1,'/Storage/Shop/1/Products/729',200.00,189.00,0,0,0,168,0.00,0.00,0,'件',4,'',0,'\0',NULL,NULL,NULL,0,0,NULL),(730,1,10,'8|9|10',0,83,0,'2017秋冬加厚韩国韩范宽松中长款毛呢外套显瘦纯色大翻领','wt005',NULL,3,1,'2017-02-14 14:43:51',1,'/Storage/Shop/1/Products/730',200.00,189.00,0,0,0,168,0.00,0.00,0,'件',3,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(731,1,37,'25|35|37',0,88,0,'蔓斯菲尔 设计师椅 简约时尚休闲塑料椅 创意电脑椅子 办公餐椅 会议椅','123456789','设计师款 缓解脊椎压力 减轻腰部劳损 时尚造型',1,2,'2017-02-14 15:32:54',1,'/Storage/Shop/1/Products/731',356.00,99.00,1,0,0,166,0.00,0.00,0,'个',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(732,1,37,'25|35|37',0,88,0,'全实木电脑桌书桌办公桌','456789',NULL,1,2,'2017-02-14 15:37:47',1,'/Storage/Shop/1/Products/732',5000.00,200.00,1,0,0,166,0.00,0.00,0,'件',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(733,1,37,'25|35|37',0,88,0,'丹麦依诺维绅 功能沙发床 时尚沙发 书房必用 小鸟','789456','丹麦设计 独立袋装弹簧 北欧时尚简约',1,2,'2017-02-14 15:44:52',1,'/Storage/Shop/1/Products/733',5000.00,4425.00,0,0,0,166,0.00,0.00,0,'件',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(734,1,37,'25|35|37',0,88,0,'原色木质简约现代艺术 家居配饰 软装配饰 可调光 木制DIY蘑菇台灯','4534543','欢迎各位客户的光顾，店铺有多个优惠促销，全场满299包邮',1,2,'2017-02-14 15:54:21',1,'/Storage/Shop/1/Products/734',100.00,63.00,0,0,0,166,0.00,0.00,0,'件',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(735,1,37,'25|35|37',0,88,0,'惠宝隆无铅水晶红酒醒酒器蜗牛形干红葡萄酒醒酒壶分酒器500ml','475674',NULL,1,2,'2017-02-14 15:59:53',1,'/Storage/Shop/1/Products/735',500.00,98.00,0,0,0,166,0.00,0.00,0,'个',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(736,1,22,'20|21|22',0,87,0,'妮维雅男士水活多效洁面乳100g','jm010',NULL,1,2,'2017-02-14 16:11:39',1,'/Storage/Shop/1/Products/736',40.00,33.00,0,0,0,167,0.00,0.00,0,'支',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(737,1,7,'1|2|7',0,82,319,'怡鲜来 法国新鲜冷冻银鳕鱼中段 200g 进口海鲜水产 深海野生鳕鱼','445',NULL,1,2,'2017-02-14 16:13:13',1,'/Storage/Shop/1/Products/737',225.00,83.00,0,0,0,166,0.00,0.00,0,'袋',0,'\0',1,'\0',NULL,NULL,NULL,0,0,NULL),(738,1,22,'20|21|22',0,87,0,'妮维雅男士水活畅透精华洁面液150ml','jm011',NULL,1,2,'2017-02-14 16:14:19',1,'/Storage/Shop/1/Products/738',55.00,35.00,0,0,0,166,0.00,0.00,0,'瓶',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(739,1,16,'14|15|16',0,84,0,'联想（Lenovo）Y50p-70 15.6英寸游戏笔记本电脑（I5-4210H 1T GTX960M升级版 ）','dn001',NULL,1,2,'2017-02-14 16:17:31',1,'/Storage/Shop/1/Products/739',5599.00,4499.00,0,0,0,166,0.00,0.00,0,'台',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(740,1,154,'1|153|154',0,82,0,'红霞草莓 3斤 单果20-30克 新鲜水果 SG','667',NULL,1,2,'2017-02-14 16:18:19',1,'/Storage/Shop/1/Products/740',30.00,25.00,0,0,0,166,0.00,0.00,0,'斤',0,'\0',10,'\0',NULL,NULL,NULL,0,0,NULL),(741,1,16,'14|15|16',0,84,0,'华硕（ASUS）FX50VX 15.6英寸游戏本（I7-6700 8G 1T GTX950M 2GB Win10 黑红）','dn002',NULL,1,2,'2017-02-14 16:20:04',1,'/Storage/Shop/1/Products/741',5899.00,5399.00,0,0,0,166,0.00,0.00,0,'台',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(742,1,19,'17|18|19',0,85,0,'怡鲜来 阿根廷红虾2000g 4斤/盒 大号L2级 40-60尾','445',NULL,1,2,'2017-02-14 16:21:08',1,'/Storage/Shop/1/Products/742',338.00,168.00,0,0,0,166,0.00,0.00,0,'斤',0,'\0',1,'\0',NULL,NULL,NULL,0,0,NULL),(743,1,156,'44|155|156',0,84,0,'勇艺达小勇机器人Y50B 太空银 家庭陪伴 启智教育 声控智能家居 视频监控','556',NULL,1,2,'2017-02-14 16:27:54',1,'/Storage/Shop/1/Products/743',3680.00,2999.00,1,0,0,166,0.00,0.00,0,'个',0,'\0',13,'\0',NULL,NULL,NULL,0,0,NULL),(744,1,157,'20|21|157',0,87,0,'SK-II 神仙水sk2护肤精华露 160ml','667',NULL,1,2,'2017-02-14 16:37:01',1,'/Storage/Shop/1/Products/744',710.00,469.00,0,0,0,166,0.00,0.00,0,'瓶',0,'\0',2,'\0',NULL,NULL,NULL,0,0,NULL),(745,1,130,'42|127|130',0,93,0,'新西兰蔓越莓蜂蜜480g 进口蜂蜜选新西兰蜂蜜品牌 NatureBeing','778',NULL,1,2,'2017-02-14 16:42:44',1,'/Storage/Shop/1/Products/745',328.00,219.00,0,0,0,166,0.00,0.00,0,'瓶',0,'\0',1,'\0',NULL,NULL,NULL,0,0,NULL),(746,1,37,'25|35|37',0,88,0,'棉麻布艺禅修加厚加大榻榻米地板圆形坐垫飘窗禅修瑜伽打坐垫55CM','123456','棉麻的面料，夏季透气不捂汗；加厚加大，填充饱满，高弹力 ',1,2,'2017-02-16 11:24:43',1,'/Storage/Shop/1/Products/746',50.00,28.00,0,0,0,167,0.00,0.00,0,'个',0,'\0',3,'\0',NULL,NULL,NULL,0,0,NULL),(747,1,37,'25|35|37',0,88,0,'北欧全实木床 简约美式实木北欧家具套装','456456','【北美进口纯实木】【全实木打造】【环保水性油漆】【环保健康】【安全无异味】',1,2,'2017-02-16 11:35:02',1,'/Storage/Shop/1/Products/747',50000.00,1000.00,0,0,0,166,0.00,0.00,0,'张',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(748,1,67,'24|50|67',0,86,0,'多功能 无网研磨 易清洗 全钢 304不锈钢 豆浆机','544335','无网全钢易清洗，食品级304不锈钢，高效直流电机，精细研磨，营养丰富！',1,2,'2017-02-16 11:46:27',1,'/Storage/Shop/1/Products/748',400.00,249.00,0,0,0,166,0.00,0.00,0,'件',0,'\0',0,'\0',NULL,NULL,NULL,0,0,NULL),(749,1,159,'151|158|159',0,95,0,'积家（Jaeger）Master Control大师系列机械男表Q1552520 银色','789456','【特价专区】店铺全部商品执行低价销售！全国联保！高价手表可咨询客服改价优惠！',1,2,'2017-02-16 12:01:19',1,'/Storage/Shop/1/Products/749',50000.00,20000.00,0,0,0,169,0.00,0.00,0,'块',0,'\0',2,'\0',NULL,NULL,NULL,0,0,NULL);
/*!40000 ALTER TABLE `mall_product` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_productattribute`
--

DROP TABLE IF EXISTS `mall_productattribute`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_productattribute` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ProductId` bigint(20) NOT NULL COMMENT '商品ID',
  `AttributeId` bigint(20) NOT NULL COMMENT '属性ID',
  `ValueId` bigint(20) NOT NULL COMMENT '属性值ID',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Attribute_ProductAttribute` (`AttributeId`) USING BTREE,
  KEY `FK_Product_ProductAttribute` (`ProductId`) USING BTREE,
  KEY `IX_ValueId` (`ValueId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=6122 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_productattribute`
--

LOCK TABLES `mall_productattribute` WRITE;
/*!40000 ALTER TABLE `mall_productattribute` DISABLE KEYS */;
INSERT INTO `mall_productattribute` VALUES (6067,702,193,813),(6069,704,194,816),(6070,705,194,819),(6071,706,194,819),(6074,709,194,816),(6076,703,193,814),(6077,701,192,810),(6079,710,194,817),(6080,699,192,811),(6081,700,192,812),(6082,711,194,817),(6083,712,194,817),(6092,718,199,837),(6093,717,199,836),(6094,717,200,839),(6095,716,196,828),(6096,716,197,830),(6097,716,198,835),(6098,715,195,824),(6099,714,195,822),(6100,708,194,817),(6101,719,199,837),(6102,720,199,836),(6103,721,200,839),(6104,722,199,837),(6105,722,200,838),(6106,723,200,838),(6108,724,194,819),(6109,726,194,819),(6110,727,194,819),(6111,728,194,819),(6112,729,194,819),(6113,730,194,819),(6115,737,193,813),(6116,738,199,836),(6117,741,195,824),(6118,736,199,836),(6119,744,200,838),(6120,745,202,843),(6121,745,203,848);
/*!40000 ALTER TABLE `mall_productattribute` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_productcomment`
--

DROP TABLE IF EXISTS `mall_productcomment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_productcomment` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `SubOrderId` bigint(20) NOT NULL COMMENT '订单详细ID',
  `ProductId` bigint(20) NOT NULL COMMENT '商品ID',
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `ShopName` varchar(100) DEFAULT NULL COMMENT '店铺名称',
  `UserId` bigint(20) NOT NULL COMMENT '用户ID',
  `UserName` varchar(100) DEFAULT NULL COMMENT '用户名称',
  `Email` varchar(1000) DEFAULT NULL COMMENT 'Email',
  `ReviewContent` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ReviewDate` datetime NOT NULL COMMENT '评价日期',
  `ReviewMark` int(11) NOT NULL COMMENT '评价说明',
  `ReplyContent` varchar(1000) DEFAULT NULL,
  `ReplyDate` datetime DEFAULT NULL,
  `AppendContent` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `AppendDate` datetime DEFAULT NULL COMMENT '追加时间',
  `ReplyAppendContent` varchar(1000) DEFAULT NULL COMMENT '追加评论回复',
  `ReplyAppendDate` datetime DEFAULT NULL COMMENT '追加评论回复时间',
  `IsHidden` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Product_ProductComment` (`ProductId`) USING BTREE,
  KEY `SubOrderId` (`SubOrderId`) USING BTREE,
  KEY `ShopId` (`ShopId`) USING BTREE,
  KEY `UserId` (`UserId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_productcomment`
--

LOCK TABLES `mall_productcomment` WRITE;
/*!40000 ALTER TABLE `mall_productcomment` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_productcomment` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_productcommentimage`
--

DROP TABLE IF EXISTS `mall_productcommentimage`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_productcommentimage` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '自增物理主键',
  `CommentImage` varchar(200) NOT NULL COMMENT '评论图片',
  `CommentId` bigint(20) NOT NULL COMMENT '评论ID',
  `CommentType` int(11) NOT NULL COMMENT '评论类型（首次评论/追加评论）',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FR_CommentImages` (`CommentId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_productcommentimage`
--

LOCK TABLES `mall_productcommentimage` WRITE;
/*!40000 ALTER TABLE `mall_productcommentimage` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_productcommentimage` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_productconsultation`
--

DROP TABLE IF EXISTS `mall_productconsultation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_productconsultation` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ProductId` bigint(20) NOT NULL,
  `ShopId` bigint(20) NOT NULL,
  `ShopName` varchar(100) DEFAULT NULL COMMENT '店铺名称',
  `UserId` bigint(20) NOT NULL,
  `UserName` varchar(100) DEFAULT NULL COMMENT '用户名称',
  `Email` varchar(1000) DEFAULT NULL,
  `ConsultationContent` varchar(1000) DEFAULT NULL COMMENT '咨询内容',
  `ConsultationDate` datetime NOT NULL COMMENT '咨询时间',
  `ReplyContent` varchar(1000) DEFAULT NULL COMMENT '回复内容',
  `ReplyDate` datetime DEFAULT NULL COMMENT '回复日期',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Product_ProductConsultation` (`ProductId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=236 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_productconsultation`
--

LOCK TABLES `mall_productconsultation` WRITE;
/*!40000 ALTER TABLE `mall_productconsultation` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_productconsultation` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_productdescription`
--

DROP TABLE IF EXISTS `mall_productdescription`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_productdescription` (
  `Id` bigint(20) NOT NULL,
  `ProductId` bigint(20) NOT NULL COMMENT '商品ID',
  `AuditReason` varchar(1000) DEFAULT NULL COMMENT '审核原因',
  `Description` text COMMENT '详情',
  `DescriptionPrefixId` bigint(20) NOT NULL COMMENT '关联版式',
  `DescriptiondSuffixId` bigint(20) NOT NULL,
  `Meta_Title` varchar(1000) DEFAULT NULL COMMENT 'SEO',
  `Meta_Description` varchar(1000) DEFAULT NULL,
  `Meta_Keywords` varchar(1000) DEFAULT NULL,
  `MobileDescription` text COMMENT '移动端描述',
  PRIMARY KEY (`ProductId`) USING BTREE,
  KEY `FK_Product_ProductDescription` (`ProductId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_productdescription`
--

LOCK TABLES `mall_productdescription` WRITE;
/*!40000 ALTER TABLE `mall_productdescription` DISABLE KEYS */;
INSERT INTO `mall_productdescription` VALUES (0,699,NULL,'<ul id=\"parameter-brand\" class=\"p-parameter-list list-paddingleft-2\"><li><p>品牌： <a href=\"https://list.jd.com/list.html?cat=1320,1583,1591&ev=exbrand_27776\" clstag=\"shangpin|keycount|product|pinpai_2\" target=\"_blank\">三只松鼠（Three Squireels）</a>\n &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;<a href=\"https://item.jd.com/1013722242.html#none\" clstag=\"shangpin|keycount|product|guanzhupinpai\" class=\"follow-brand btn-def\"><strong>♥</strong>关注</a></p></li></ul><ul id=\"parameter2\" class=\"p-parameter-list list-paddingleft-2\"><li><p>商品名称：三只松鼠_开口松子218gx2袋零食坚果特产炒货东北红松子原味</p></li><li><p>商品编号：1013722242</p></li><li><p>店铺： <a href=\"https://3songshu.jd.com/\" target=\"_blank\">三只松鼠旗舰店</a></p></li><li><p>商品毛重：436.00g</p></li><li><p>货号：D6956511900336</p></li><li><p>口味：原味</p></li><li><p>价位：30-59</p></li><li><p>特性：带皮</p></li><li><p>国产/进口：国产</p></li><li><p>分类：松子</p></li><li><p>包装：其它　</p></li><li><p>包装单位：袋装</p></li></ul><p class=\"more-par\"><img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2896/179/505289072/230040/a848de99/57175623N72c7af09.jpg\" alt=\"\" id=\"c31b74f5d0914a26babaeb7be5e30b25\"/>&nbsp; \n<img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2740/55/517409922/322817/a718d4a3/57175624Na185d8fd.jpg\" alt=\"\" id=\"96b889a1e3fe4d978aedcbfa1f990e19\"/><br/></p><p><img src=\"/Storage/Shop/1/Products/699/remark/d7ba33dba2604266948cf04ddf8fb4f6.jpg\" alt=\"\" id=\"703ae2825c3f45778006bcdffbb0fe29\"/><br/><img src=\"/Storage/Shop/1/Products/699/remark/191d7e6a43654ce8b2280c27380564a8.gif\" alt=\"\" id=\"a23768f1ef5247f19f3f46f0bbb5621f\"/><br/><img src=\"/Storage/Shop/1/Products/699/remark/9fc896b22b1f4d26b581d7b3f95fd190.jpg\" alt=\"\" id=\"f870a148d5644cef9d77964dd8464fc8\"/><br/><img src=\"/Storage/Shop/1/Products/699/remark/2e5289f656fa41ceb8ea76f020ac8057.jpg\" alt=\"\" id=\"2659821d3e50424bb5da676954720e01\"/><br/><br/><br/><img src=\"/Storage/Shop/1/Products/699/remark/97917f77ad1c473b94ffcd4dd7107a1d.jpg\" alt=\"\" id=\"6c992aadac684e7088ebbb8ead53d017\"/><br/><img src=\"/Storage/Shop/1/Products/699/remark/323cb8c17231453388cd4feb6b3ec6c1.jpg\" alt=\"\" id=\"8ad9e83975284676bcb3018de83696f1\"/></p>',0,0,NULL,NULL,NULL,'<ul id=\"parameter-brand\" class=\"p-parameter-list list-paddingleft-2\"><li><p>品牌： <a href=\"https://list.jd.com/list.html?cat=1320,1583,1591&ev=exbrand_27776\" clstag=\"shangpin|keycount|product|pinpai_2\" target=\"_blank\">三只松鼠（Three Squireels）</a>\n &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;<a href=\"https://item.jd.com/1013722242.html#none\" clstag=\"shangpin|keycount|product|guanzhupinpai\" class=\"follow-brand btn-def\"><strong>♥</strong>关注</a></p></li></ul><ul id=\"parameter2\" class=\"p-parameter-list list-paddingleft-2\"><li><p>商品名称：三只松鼠_开口松子218gx2袋零食坚果特产炒货东北红松子原味</p></li><li><p>商品编号：1013722242</p></li><li><p>店铺： <a href=\"https://3songshu.jd.com/\" target=\"_blank\">三只松鼠旗舰店</a></p></li><li><p>商品毛重：436.00g</p></li><li><p>货号：D6956511900336</p></li><li><p>口味：原味</p></li><li><p>价位：30-59</p></li><li><p>特性：带皮</p></li><li><p>国产/进口：国产</p></li><li><p>分类：松子</p></li><li><p>包装：其它　</p></li><li><p>包装单位：袋装</p></li></ul><p class=\"more-par\"><a href=\"https://item.jd.com/1013722242.html#product-detail\" class=\"J-more-param\">更多参数&gt;&gt;</a></p><p><br/></p><table cellspacing=\"0\" cellpadding=\"0\" width=\"990\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td colspan=\"4\"><a href=\"https://3songshu.jd.com/\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/699/remark/15e7dd618a5746409c4d45ddba479616.jpg\" width=\"990\" height=\"395\"/></a></td></tr><tr><td><a href=\"https://coupon.jd.com/ilink/couponActiveFront/front_index.action?key=5990c0f2cd434a1e9e6d742ef4f6e1aa&roleId=5476594&to=3songshu.jd.com\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/699/remark/e78448388eb343bebb8197df77cadb3f.jpg\" width=\"248\" height=\"162\"/></a></td><td><a href=\"https://coupon.jd.com/ilink/couponActiveFront/front_index.action?key=8ab86e0bf1104170b5afdd223e06d8e3&roleId=5476598&to=3songshu.jd.com\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/699/remark/de95e4db91ad4e799d078249b963d1e6.jpg\" width=\"248\" height=\"162\"/></a></td><td><a href=\"https://sale.jd.com/act/GpvHiy6rSxjYd1NT.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/699/remark/4d01d45845e240e3bccac652aedd537d.jpg\" width=\"248\" height=\"162\"/></a></td><td><a href=\"https://sale.jd.com/act/zlbyo5YPFsp.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/699/remark/e2b2c894318d4164a03060b26f51c68d.jpg\" width=\"246\" height=\"162\"/></a></td></tr><tr><td><a href=\"https://item.jd.com/1014341394.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/699/remark/e15455a9770f404c8f97f5be08c3924a.jpg\"/></a></td><td><a href=\"https://item.jd.com/1018421638.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/699/remark/47b8dc6e31da4d269fb08431ff74cfc7.jpg\"/></a></td><td><a href=\"https://item.jd.com/10061035713.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/699/remark/47cf5e3414eb4572b0be438694d97f56.jpg\" width=\"248\" height=\"270\"/></a></td><td><a href=\"https://item.jd.com/10900859415.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/699/remark/b7bfbd55b7194f8cb26a1807e8a6c34a.jpg\" width=\"246\" height=\"270\"/></a></td></tr><tr><td><a href=\"https://item.jd.com/1582161447.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/699/remark/33ae1888ff794c95bd1dd46a8c49446a.jpg\" width=\"248\" height=\"280\"/></a></td><td><a href=\"https://item.jd.com/1601589482.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/699/remark/6e6575f29f384cb2900cddbfcccaf072.jpg\" width=\"248\" height=\"280\"/></a></td><td><a href=\"https://item.jd.com/1601632347.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/699/remark/9e42d5a8bc194515bd1fbd7308c7bd2e.jpg\" width=\"248\" height=\"280\"/></a></td><td><a href=\"https://item.jd.com/10422380333.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/699/remark/f90d704c604b484fa0e8c50bde8bba0f.jpg\" width=\"246\" height=\"280\"/></a></td></tr></tbody></table><p><br/> <img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2245/136/2729287964/322296/b680fe4/57175621Nbd4579e3.jpg\" alt=\"\" id=\"00e007caa37e4b25b575fbd254717de2\"/> <br/> <img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t1951/76/2727596032/343744/184b0fc2/57175622Nd678d2bf.jpg\" alt=\"\" id=\"efb55fbeed4e4ba6b3bb1b2781f7c4aa\"/> <br/> <img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2875/147/518214398/282404/3080f341/57175622Neaa9da14.jpg\" alt=\"\" id=\"80c4974024794684808b46e9bc92cc01\"/> <br/> <img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2158/340/2790905661/192550/f04844eb/57175622Neb3b4a1a.jpg\" alt=\"\" id=\"ff1164dc67784b719e4afedb14ea4182\"/> <br/> <img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2716/225/515636023/365982/cf73b87d/57175623Ne40c1a7c.jpg\" alt=\"\" id=\"178501f9309f4f1a88b79bd0c399683a\"/> <br/> <img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2803/307/513296203/265046/cd0b0eb2/57175623Nbfc86251.jpg\" alt=\"\" id=\"967e7b85f87b4a9d848e1723693144b7\"/> <br/> <img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2896/179/505289072/230040/a848de99/57175623N72c7af09.jpg\" alt=\"\" id=\"c31b74f5d0914a26babaeb7be5e30b25\"/>&nbsp; \n<img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2740/55/517409922/322817/a718d4a3/57175624Na185d8fd.jpg\" alt=\"\" id=\"96b889a1e3fe4d978aedcbfa1f990e19\"/><br/></p><p><img src=\"/Storage/Shop/1/Products/699/remark/9735abe458c947c980798d4afba62afa.jpg\" alt=\"\" id=\"703ae2825c3f45778006bcdffbb0fe29\"/><br/><img src=\"/Storage/Shop/1/Products/699/remark/a533973ddced4c23ab0796541b38d42d.gif\" alt=\"\" id=\"a23768f1ef5247f19f3f46f0bbb5621f\"/><br/><img src=\"/Storage/Shop/1/Products/699/remark/f916a971b22544d09174b3cad40d57b1.jpg\" alt=\"\" id=\"f870a148d5644cef9d77964dd8464fc8\"/><br/><img src=\"/Storage/Shop/1/Products/699/remark/3b8cd6817ef64183aa3da95c6e7bb9be.jpg\" alt=\"\" id=\"2659821d3e50424bb5da676954720e01\"/><br/><br/><br/><img src=\"/Storage/Shop/1/Products/699/remark/950ba545abfc4109b4a9e96c5bd0ee28.jpg\" alt=\"\" id=\"6c992aadac684e7088ebbb8ead53d017\"/><br/><img src=\"/Storage/Shop/1/Products/699/remark/a340088180664b4597f7744bdc555385.jpg\" alt=\"\" id=\"8ad9e83975284676bcb3018de83696f1\"/></p>'),(0,700,NULL,'<ul id=\"parameter-brand\" class=\"p-parameter-list list-paddingleft-2\"><li><p>品牌： <a href=\"https://list.jd.com/list.html?cat=1320,1583,1591&ev=exbrand_27776\" clstag=\"shangpin|keycount|product|pinpai_2\" target=\"_blank\">三只松鼠（Three Squireels）</a>\n &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;<a href=\"https://item.jd.com/1014341394.html#none\" clstag=\"shangpin|keycount|product|guanzhupinpai\" class=\"follow-brand btn-def\"><strong>♥</strong>关注</a></p></li><li><p>商品名称：三只松鼠_碧根果210gx2袋零食坚果炒货山核桃长寿果干果奶油味</p></li><li><p>商品编号：1014341394</p></li><li><p>店铺： <a href=\"https://3songshu.jd.com/\" target=\"_blank\">三只松鼠旗舰店</a></p></li><li><p>商品毛重：420.00g</p></li><li><p>口味：奶油味</p></li><li><p>价位：30-59</p></li><li><p>特性：带皮</p></li><li><p>国产/进口：国产</p></li><li><p>分类：长寿果/碧根果</p></li><li><p>包装：其它　</p></li><li><p>包装单位：袋装</p></li></ul><ul id=\"parameter2\" class=\"p-parameter-list list-paddingleft-2\"></ul><p><img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2962/256/1352317913/359033/f9e92b49/577f69b9N44427198.jpg\" alt=\"\" id=\"f6a2a587fa6e4f6080440347845954aa\"/> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;<br/></p><p><img src=\"/Storage/Shop/1/Products/700/remark/4d55a51dd68d4425b6076cc536b14c7c.jpg\" alt=\"\" id=\"703ae2825c3f45778006bcdffbb0fe29\"/><br/><img src=\"/Storage/Shop/1/Products/700/remark/434441ee2ff048aa816cd13c26dd4d00.gif\" alt=\"\" id=\"a23768f1ef5247f19f3f46f0bbb5621f\"/><br/><img src=\"/Storage/Shop/1/Products/700/remark/2246ef998b9c4fcda2867d498a00145d.jpg\" alt=\"\" id=\"f870a148d5644cef9d77964dd8464fc8\"/><br/><img src=\"/Storage/Shop/1/Products/700/remark/651735812f9a4b04917d777b13addf75.jpg\" alt=\"\" id=\"2659821d3e50424bb5da676954720e01\"/><br/><br/><br/><img src=\"/Storage/Shop/1/Products/700/remark/d735674e96574c23afc6ce07435737c3.jpg\" alt=\"\" id=\"6c992aadac684e7088ebbb8ead53d017\"/><br/><img src=\"/Storage/Shop/1/Products/700/remark/44117f01d1f841b9a62cf55c4c325739.jpg\" alt=\"\" id=\"8ad9e83975284676bcb3018de83696f1\"/></p>',0,0,NULL,NULL,NULL,'<ul id=\"parameter-brand\" class=\"p-parameter-list list-paddingleft-2\"><li><p>品牌： <a href=\"https://list.jd.com/list.html?cat=1320,1583,1591&ev=exbrand_27776\" clstag=\"shangpin|keycount|product|pinpai_2\" target=\"_blank\">三只松鼠（Three Squireels）</a>\n &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;<a href=\"https://item.jd.com/1014341394.html#none\" clstag=\"shangpin|keycount|product|guanzhupinpai\" class=\"follow-brand btn-def\"><strong>♥</strong>关注</a></p></li></ul><ul id=\"parameter2\" class=\"p-parameter-list list-paddingleft-2\"><li><p>商品名称：三只松鼠_碧根果210gx2袋零食坚果炒货山核桃长寿果干果奶油味</p></li><li><p>商品编号：1014341394</p></li><li><p>店铺： <a href=\"https://3songshu.jd.com/\" target=\"_blank\">三只松鼠旗舰店</a></p></li><li><p>商品毛重：420.00g</p></li><li><p>口味：奶油味</p></li><li><p>价位：30-59</p></li><li><p>特性：带皮</p></li><li><p>国产/进口：国产</p></li><li><p>分类：长寿果/碧根果</p></li><li><p>包装：其它　</p></li><li><p>包装单位：袋装</p></li></ul><table cellspacing=\"0\" cellpadding=\"0\" width=\"990\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td colspan=\"4\"><a href=\"https://3songshu.jd.com/\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/700/remark/79267d0140574797bdd8c7771a0a5a89.jpg\" width=\"990\" height=\"395\"/></a></td></tr><tr><td><a href=\"https://coupon.jd.com/ilink/couponActiveFront/front_index.action?key=5990c0f2cd434a1e9e6d742ef4f6e1aa&roleId=5476594&to=3songshu.jd.com\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/700/remark/e2e8af6487a94144a59b28dd30aa7db5.jpg\" width=\"248\" height=\"162\"/></a></td><td><a href=\"https://coupon.jd.com/ilink/couponActiveFront/front_index.action?key=8ab86e0bf1104170b5afdd223e06d8e3&roleId=5476598&to=3songshu.jd.com\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/700/remark/3d84ac70d34c47fa8b195b9c35bbd444.jpg\" width=\"248\" height=\"162\"/></a></td><td><a href=\"https://sale.jd.com/act/GpvHiy6rSxjYd1NT.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/700/remark/3e00e92f176248d39de9f2c8fafd5ce3.jpg\" width=\"248\" height=\"162\"/></a></td><td><a href=\"https://sale.jd.com/act/zlbyo5YPFsp.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/700/remark/f3b1ff90de8e4f498a545ef9fc5264fe.jpg\" width=\"246\" height=\"162\"/></a></td></tr><tr><td><a href=\"https://item.jd.com/1014341394.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/700/remark/59c2ff30c80e4d1baec4522f4ca91cb6.jpg\"/></a></td><td><a href=\"https://item.jd.com/1018421638.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/700/remark/f3c43e7884ea413ba2f36bd480135ffc.jpg\"/></a></td><td><a href=\"https://item.jd.com/10061035713.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/700/remark/637d595d9c3b42c4bb5eba6dcd9598fd.jpg\" width=\"248\" height=\"270\"/></a></td><td><a href=\"https://item.jd.com/10900859415.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/700/remark/2af11750981449478ce5431a498b31c7.jpg\" width=\"246\" height=\"270\"/></a></td></tr><tr><td><a href=\"https://item.jd.com/1582161447.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/700/remark/52dcf10318d54b348101188dc6e53d59.jpg\" width=\"248\" height=\"280\"/></a></td><td><a href=\"https://item.jd.com/1601589482.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/700/remark/abea2564535c4c11baff926bc8afab84.jpg\" width=\"248\" height=\"280\"/></a></td><td><a href=\"https://item.jd.com/1601632347.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/700/remark/cd6f1f918b2d47fd9eeff6cdae5fccf6.jpg\" width=\"248\" height=\"280\"/></a></td><td><a href=\"https://item.jd.com/10422380333.html\" target=\"_blank\"><img alt=\"\" src=\"/Storage/Shop/1/Products/700/remark/ce71588e28954557a9762c175ad980e7.jpg\" width=\"246\" height=\"280\"/></a></td></tr></tbody></table><p><br/> <img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2962/256/1352317913/359033/f9e92b49/577f69b9N44427198.jpg\" alt=\"\" id=\"f6a2a587fa6e4f6080440347845954aa\"/> <br/> <img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2857/172/3153596383/366743/6beec3a3/577f69baNcdf6fd21.jpg\" alt=\"\" id=\"2f3f3f21793144a7966928be4522cb2a\"/> <br/> <img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2767/205/3071018575/258802/e916e77b/577f69baN732dec17.jpg\" alt=\"\" id=\"b8d4181ad0ec415681f8b988a1136cbf\"/> <br/> <img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2659/156/3068442824/290977/338a8272/577f69baN8421c8d2.jpg\" alt=\"\" id=\"858493231faa437994eadeb3393b9af0\"/> <br/> <img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2713/214/3061292493/283475/5c4b26a2/577f69bbN24b6fe15.jpg\" alt=\"\" id=\"78240579295648a1987d43a63a7294d9\"/> <br/> <img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2836/213/3014207267/240880/ac2c122c/577f69bbN12f3f496.jpg\" alt=\"\" id=\"eafeed7018004872bdd9b8addb047faf\"/> <br/> <img data-lazyload=\"//img30.360buyimg.com/popWareDetail/jfs/t2848/255/3065723508/365685/73b154ba/577f69bcN279b563c.jpg\" alt=\"\" id=\"3d1726d2fc4741529b2a4ea0762bb0fe\"/>\n &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;<br/></p><p><img src=\"/Storage/Shop/1/Products/700/remark/18aa0715b15a4f4cba60df6defab6132.jpg\" alt=\"\" id=\"703ae2825c3f45778006bcdffbb0fe29\"/><br/><img src=\"/Storage/Shop/1/Products/700/remark/d9d4c9f3f94f4b62bd0d41caf75f3fd9.gif\" alt=\"\" id=\"a23768f1ef5247f19f3f46f0bbb5621f\"/><br/><img src=\"/Storage/Shop/1/Products/700/remark/444f3ed84f154aa18241d373d04675ab.jpg\" alt=\"\" id=\"f870a148d5644cef9d77964dd8464fc8\"/><br/><img src=\"/Storage/Shop/1/Products/700/remark/06978b1ffd784cdc9b10a8ab6e8b4f52.jpg\" alt=\"\" id=\"2659821d3e50424bb5da676954720e01\"/><br/><br/><br/><img src=\"/Storage/Shop/1/Products/700/remark/f9a927d07e8b4e7f84d5ae76ebbcce4f.jpg\" alt=\"\" id=\"6c992aadac684e7088ebbb8ead53d017\"/><br/><img src=\"/Storage/Shop/1/Products/700/remark/9f712a42a89a435aabcfc5d453ebb8b4.jpg\" alt=\"\" id=\"8ad9e83975284676bcb3018de83696f1\"/></p>'),(0,701,NULL,'<ul id=\"parameter-brand\" class=\"p-parameter-list list-paddingleft-2\"><li><p>品牌： <a href=\"https://list.jd.com/list.html?cat=1320,1583,1591&ev=exbrand_27776\" clstag=\"shangpin|keycount|product|pinpai_2\" target=\"_blank\">三只松鼠（Three Squireels）</a>\n &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;<a href=\"https://item.jd.com/2518067.html#none\" clstag=\"shangpin|keycount|product|guanzhupinpai\" class=\"follow-brand btn-def\"><strong>♥</strong>关注</a></p></li></ul><ul id=\"parameter2\" class=\"p-parameter-list list-paddingleft-2\"><li><p>商品名称：三只松鼠纸皮核桃</p></li><li><p>商品编号：2518067</p></li><li><p>商品毛重：210.00g</p></li><li><p>商品产地：浙江省</p></li><li><p>口味：原味</p></li><li><p>特性：带皮</p></li><li><p>国产/进口：国产</p></li><li><p>价位：10-29</p></li><li><p>分类：核桃</p></li><li><p>包装单位：袋装</p></li></ul><p class=\"more-par\"><a href=\"https://item.jd.com/2518067.html#product-detail\" class=\"J-more-param\">更多参数&gt;&gt;</a></p><p><br/></p><p><a target=\"_blank\" href=\"https://sale.jd.com/act/ZnDo10XW3q.html\"><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/45f30c939842413b9dfaf007e147fb07.jpg\"/></a></p><table id=\"__01\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"750\" height=\"4725\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/5ba122f4cbe94aeb92ff39826aaae5e4.jpg\" width=\"750\" height=\"505\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/9dbf992cf0b04c6e8a1439f35a8ab901.jpg\" width=\"750\" height=\"627\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/935742a9263d44c5b06e56311071bb59.jpg\" width=\"750\" height=\"544\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/3e41f087260b4e46a0f4549833fc1560.jpg\" width=\"750\" height=\"580\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/caf5e9df00e24f839ab43d29f8d1a27a.jpg\" width=\"750\" height=\"451\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/1bb5d9fe1b4d4e378902107de1a89e41.jpg\" width=\"750\" height=\"528\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/f3610c346e184469bac0bccddfb445d5.jpg\" width=\"750\" height=\"444\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/21e0077b40a44b3fbac317172042f267.jpg\" width=\"750\" height=\"485\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/b27d038feb2f46cebf4db15fc2415351.jpg\" width=\"750\" height=\"561\"/></td></tr></tbody></table>',0,0,NULL,NULL,NULL,'<ul id=\"parameter-brand\" class=\"p-parameter-list list-paddingleft-2\"><li><p>品牌： <a href=\"https://list.jd.com/list.html?cat=1320,1583,1591&ev=exbrand_27776\" clstag=\"shangpin|keycount|product|pinpai_2\" target=\"_blank\">三只松鼠（Three Squireels）</a>\n &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;<a href=\"https://item.jd.com/2518067.html#none\" clstag=\"shangpin|keycount|product|guanzhupinpai\" class=\"follow-brand btn-def\"><strong>♥</strong>关注</a></p></li></ul><ul id=\"parameter2\" class=\"p-parameter-list list-paddingleft-2\"><li><p>商品名称：三只松鼠纸皮核桃</p></li><li><p>商品编号：2518067</p></li><li><p>商品毛重：210.00g</p></li><li><p>商品产地：浙江省</p></li><li><p>口味：原味</p></li><li><p>特性：带皮</p></li><li><p>国产/进口：国产</p></li><li><p>价位：10-29</p></li><li><p>分类：核桃</p></li><li><p>包装单位：袋装</p></li></ul><p class=\"more-par\"><a href=\"https://item.jd.com/2518067.html#product-detail\" class=\"J-more-param\">更多参数&gt;&gt;</a></p><p><br/></p><p><a target=\"_blank\" href=\"https://sale.jd.com/act/ZnDo10XW3q.html\"><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/a2c4c9e794c0425e963d0cfa08e6e8e8.jpg\"/></a></p><table id=\"__01\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"750\" height=\"4725\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/4b5680e49b0240e2a1dd39b13b2cd6bf.jpg\" width=\"750\" height=\"505\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/5f8202f4d8374d7a93b3b0db52b29eb5.jpg\" width=\"750\" height=\"627\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/dffd8c9fbb9d44a8a54e9cf68d08c863.jpg\" width=\"750\" height=\"544\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/5236817bd887409db17494be1ae632b5.jpg\" width=\"750\" height=\"580\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/32f827e4d32a49039613dcc7d1a3e68f.jpg\" width=\"750\" height=\"451\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/b702549151a0435abfbf9b7d5cd68fec.jpg\" width=\"750\" height=\"528\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/50e964e479eb43d88447a285fa1e83fa.jpg\" width=\"750\" height=\"444\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/ef4eafe237124d68aafb31dc81a0f8a2.jpg\" width=\"750\" height=\"485\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/701/remark/e94f4cafaa32478aad74d4fef87406d9.jpg\" width=\"750\" height=\"561\"/></td></tr></tbody></table>'),(0,702,NULL,'<p>产品信息 &nbsp; &nbsp;<span class=\"s2\">Product Information</span>\n &nbsp;</p><table cellspacing=\"6\" cellpadding=\"0\" align=\"center\" width=\"750\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td><img data-lazyload=\"done\" src=\"/Storage/Shop/1/Products/702/remark/cda4ec6fc4334447a2b579e89c735915.jpg\"/></td><td><p class=\"formwork_titleleft\">卫龙 休闲零食 面筋（辣条） 22g*20包</p><p class=\"formwork_titleleft2\">品牌： 卫龙<br/>配料： 小麦粉，生活饮用水，植物油，食用盐，白砂糖， \n香辛料，食品添加剂（谷氨酸钠、单硬脂酸甘油酯、甜蜜素、5‘-呈味甘酸二钠、阿斯巴甜（含苯丙氨酸）、复配糕点防腐剂（水溶（脱氢乙酸钠、柠檬酸钠、葡萄糖酸-δ-内脂））、复配糕点防腐剂（脂溶（山梨酸、蒸馏单硬脂酸甘油酯、蔗糖脂肪酸酯））、三氯蔗糖、糖精钠、特丁基对苯二酚、食用香精）<br/>净含量： 440g<br/>保质期： 120天<br/>储存方法： 阴凉干燥处<br/>生产许可证号： QS411107010002<br/>产品执行标准：DB41/T 515<br/>产地：河南漯河市<br/>致敏源：本品含辣椒、小麦及其制品、大豆及其制品，可能导致过敏<br/></p></td></tr></tbody></table><p>产品展示 &nbsp; &nbsp;<span class=\"s2\">Products Exhibition</span>\n &nbsp;</p><p><img data-lazyload=\"done\" src=\"/Storage/Shop/1/Products/702/remark/807329c1150d44e0903d34bc953dade0.jpg\"/></p><p><img data-lazyload=\"done\" src=\"/Storage/Shop/1/Products/702/remark/5796f9c809064699b0f9d6621cc8084f.jpg\"/></p><p><img data-lazyload=\"done\" src=\"/Storage/Shop/1/Products/702/remark/687ffef7b9f0483c889f6d4901c59aec.jpg\"/></p><p><img data-lazyload=\"done\" src=\"/Storage/Shop/1/Products/702/remark/8504be201230441293a4d674c6510496.jpg\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p>产品信息 &nbsp; &nbsp;<span class=\"s2\">Product Information</span>\n &nbsp;</p><table cellspacing=\"6\" cellpadding=\"0\" align=\"center\" width=\"750\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td><img data-lazyload=\"done\" src=\"/Storage/Shop/1/Products/702/remark/42ee086b138e4c8dbae073780b78994f.jpg\"/></td><td><p class=\"formwork_titleleft\">卫龙 休闲零食 面筋（辣条） 22g*20包</p><p class=\"formwork_titleleft2\">品牌： 卫龙<br/>配料： 小麦粉，生活饮用水，植物油，食用盐，白砂糖， \n香辛料，食品添加剂（谷氨酸钠、单硬脂酸甘油酯、甜蜜素、5‘-呈味甘酸二钠、阿斯巴甜（含苯丙氨酸）、复配糕点防腐剂（水溶（脱氢乙酸钠、柠檬酸钠、葡萄糖酸-δ-内脂））、复配糕点防腐剂（脂溶（山梨酸、蒸馏单硬脂酸甘油酯、蔗糖脂肪酸酯））、三氯蔗糖、糖精钠、特丁基对苯二酚、食用香精）<br/>净含量： 440g<br/>保质期： 120天<br/>储存方法： 阴凉干燥处<br/>生产许可证号： QS411107010002<br/>产品执行标准：DB41/T 515<br/>产地：河南漯河市<br/>致敏源：本品含辣椒、小麦及其制品、大豆及其制品，可能导致过敏<br/></p></td></tr></tbody></table><p>产品展示 &nbsp; &nbsp;<span class=\"s2\">Products Exhibition</span>\n &nbsp;</p><p><img data-lazyload=\"done\" src=\"/Storage/Shop/1/Products/702/remark/f5c5df0dd41248ef9ca110cd726c7c22.jpg\"/></p><p><img data-lazyload=\"done\" src=\"/Storage/Shop/1/Products/702/remark/c22c45fac120414698d6c54de70dbb3b.jpg\"/></p><p><img data-lazyload=\"done\" src=\"/Storage/Shop/1/Products/702/remark/7bde798d0fcb43669fc81253ad6693fe.jpg\"/></p><p><img data-lazyload=\"done\" src=\"/Storage/Shop/1/Products/702/remark/8aec34eabbd04416ae67db6d0329e231.jpg\"/></p><p><br/></p>'),(0,703,NULL,'<table id=\"__01\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"750\" height=\"2879\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/703/remark/1fd12792fee34e6f8d079b9e17865af3.jpg\" width=\"750\" height=\"498\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/703/remark/bd9b29648b6749a3b4432edfd4fcb508.jpg\" width=\"750\" height=\"523\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/703/remark/19e09bc1705e43bea6ac6f2ad8536aca.jpg\" width=\"750\" height=\"369\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/703/remark/d9141ed772fb4b2f98250b72a26cbac4.jpg\" width=\"750\" height=\"681\"/></td></tr></tbody></table>',0,0,NULL,NULL,NULL,'<table id=\"__01\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"750\" height=\"2879\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/703/remark/eda89b3e5d6d4ff2b5c18545381919d5.jpg\" width=\"750\" height=\"498\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/703/remark/ade31a857cd04e79b7ba6e840ce3eedd.jpg\" width=\"750\" height=\"523\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/703/remark/0a248c66d6e2499d888b955d74ed91c5.jpg\" width=\"750\" height=\"369\"/></td></tr><tr><td><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/703/remark/8d8741330a634584b320aeb8e05285fc.jpg\" width=\"750\" height=\"681\"/></td></tr></tbody></table>'),(0,704,NULL,'<p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/704/remark/73462193f99247a1b7f19d517f225fac.jpg\"/></p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/704/remark/034fb1533972467c815fbb42ea4ad873.jpg\" width=\"730\" height=\"1046\"/></p><p>尺码 &nbsp; <span style=\"font-size:14px; padding-left:20px;\">因不同的计量方法，测量允许1-3cm内误差（CM）</span></p><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">身高/净腰围/型号</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">肩宽</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">胸围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">腰围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">衣长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">袖长</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">155/76A/XS</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">34.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">92</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">63</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">58</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">160/80A/S</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">35</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">94</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">64</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">59</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">165/84A/M</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">36</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">98</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">66</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">60</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">170/88A/L</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">37</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">102</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">68</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">61</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/92A/XL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">38</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">106</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">69</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">62</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/96A/XXL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">39</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">110</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">70</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">62</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"font-size:24px; line-height:24px;\" width=\"120\">尺码参考</td><td width=\"44\"><br/></td><td width=\"175\">模特身高179</td><td width=\"91\">胸围82</td><td width=\"103\">臀围88</td><td width=\"98\">腰围60</td><td align=\"right\" width=\"99\">穿着M码</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>设计卖点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>● &nbsp; &nbsp; &nbsp; &nbsp; <span style=\"padding-left:12px;\">宽松版型</span></p><p>● &nbsp; &nbsp; &nbsp; &nbsp; <span style=\"padding-left:12px;\">针织面料</span></p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px; border-bottom-color: rgb(204, 204, 204); padding-bottom: 50px;\" width=\"120\" valign=\"top\"><p>商品指数</p></td><td style=\"border-bottom-color: rgb(204, 204, 204); padding-bottom: 50px;\" width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px; border-bottom-color: rgb(204, 204, 204); padding-bottom: 50px;\" width=\"566\">薄厚指数轻薄薄适中厚加厚</td></tr></tbody></table><p><br/></p>',0,0,NULL,NULL,NULL,'<p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/704/remark/4f1f7449c1224c3cab0de059bcbcc9f8.jpg\"/></p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/704/remark/d88018685655476ca9effb1808d2ecfb.jpg\" width=\"730\" height=\"1046\"/></p><p>尺码 &nbsp; <span style=\"font-size:14px; padding-left:20px;\">因不同的计量方法，测量允许1-3cm内误差（CM）</span></p><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">身高/净腰围/型号</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">肩宽</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">胸围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">腰围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">衣长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">袖长</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">155/76A/XS</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">34.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">92</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">63</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">58</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">160/80A/S</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">35</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">94</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">64</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">59</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">165/84A/M</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">36</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">98</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">66</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">60</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">170/88A/L</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">37</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">102</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">68</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">61</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/92A/XL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">38</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">106</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">69</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">62</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/96A/XXL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">39</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">110</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">70</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">62</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"font-size:24px; line-height:24px;\" width=\"120\">尺码参考</td><td width=\"44\"><br/></td><td width=\"175\">模特身高179</td><td width=\"91\">胸围82</td><td width=\"103\">臀围88</td><td width=\"98\">腰围60</td><td align=\"right\" width=\"99\">穿着M码</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>设计卖点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>● &nbsp; &nbsp; &nbsp; &nbsp; <span style=\"padding-left:12px;\">宽松版型</span></p><p>● &nbsp; &nbsp; &nbsp; &nbsp; <span style=\"padding-left:12px;\">针织面料</span></p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px; border-bottom-color: rgb(204, 204, 204); padding-bottom: 50px;\" width=\"120\" valign=\"top\"><p>商品指数</p></td><td style=\"border-bottom-color: rgb(204, 204, 204); padding-bottom: 50px;\" width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px; border-bottom-color: rgb(204, 204, 204); padding-bottom: 50px;\" width=\"566\">薄厚指数轻薄薄适中厚加厚</td></tr></tbody></table><p><br/></p>'),(0,705,NULL,'<p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/705/remark/5d6ce54efb8a472ea17fc280a7d1b9a1.jpg\"/> \n </p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/705/remark/a7718da5072b4c2a8b4f8387eeadc67d.jpg\" width=\"730\" height=\"1046\"/> \n </p><p>本款更多颜色选择</p><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/705/remark/1796b2bdc00b4151b20c05e2a96bddc4.jpg\" width=\"365\" height=\"584\"/></td><td><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/705/remark/58d0cb67b1524b60ac6f2caa3f94e7e2.jpg\" width=\"365\" height=\"584\"/></td></tr><tr style=\"text-align: center; line-height: 20px; font-size: 20px;\"><td style=\"padding-top: 30px; padding-bottom: 40px;\">黑色</td><td style=\"padding-top: 30px; padding-bottom: 40px;\">庆典红</td></tr></tbody></table><p>尺码 \n &nbsp;<span style=\"padding-left: 20px; font-size: 14px;\">因不同的计量方法，测量允许1-3cm内误差（CM）</span> \n </p><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">身高/净腰围/型号</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">肩宽</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">胸围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">腰围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">衣长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">袖长</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">155/76A/XS</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">56</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">104</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">66/68/53</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">46</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">160/80A/S</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">57</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">106</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">67/69/54</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">46.5</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">165/84A/M</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">59</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">110</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">69/71/56</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">47</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">170/88A/L</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">61</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">114</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">71/73/58</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">47.5</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/92A/XL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">63</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">118</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">72/74/59</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">48</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/96A/XXL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">65</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">122</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">73/75/60</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">48</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"line-height: 24px; font-size: 24px;\" width=\"120\">尺码参考</td><td width=\"44\"><br/></td><td width=\"175\">模特身高179</td><td width=\"91\">胸围82</td><td width=\"103\">臀围88</td><td width=\"98\">腰围60</td><td align=\"right\" width=\"99\">穿着M码</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"padding-top: 30px; border-top-color: rgb(204, 204, 204);\" width=\"120\" valign=\"top\"><p>设计卖点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>● \n &nbsp; &nbsp; &nbsp; &nbsp;<span style=\"padding-left: 12px;\">舒适面料 </span> \n &nbsp; &nbsp; &nbsp;</p><p>● \n &nbsp; &nbsp; &nbsp; &nbsp;<span style=\"padding-left: 12px;\">宽松版型</span> \n &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"padding-top: 30px; padding-bottom: 50px; border-top-color: rgb(204, 204, 204); border-bottom-color: rgb(204, 204, 204);\" width=\"120\" valign=\"top\"><p>商品指数</p></td><td style=\"padding-bottom: 50px; border-bottom-color: rgb(204, 204, 204);\" width=\"44\"><br/></td><td style=\"padding-top: 30px; padding-bottom: 50px; border-top-color: rgb(204, 204, 204); border-bottom-color: rgb(204, 204, 204);\" width=\"566\">薄厚指数轻薄薄适中厚加厚</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" background=\"http://ecphoto.bestseller.com.cn/onlyimages/2016_Q3/117124502E40_dp.jpg\" width=\"730\" height=\"800\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td width=\"455\" height=\"30\"><br/></td><td><br/></td><td width=\"50\"><br/></td></tr><tr><td width=\"455\"><br/></td><td bgcolor=\"#ffffff\"><p><span style=\"width: 225px; font-size: 20px; margin-bottom: 15px; display: block;\">成套搭配购买</span> \n &nbsp; &nbsp; &nbsp;</p><p><a style=\"color: rgb(0, 0, 0); text-decoration: none;\" href=\"https://item.jd.com/1587028479.html\" target=\"_blank\"><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/705/remark/03b8524906154ef0aebf286b752bb1ae.jpg\" width=\"120\" height=\"73\"/> <span style=\"width: 225px; margin-top: 8px; display: block;\">吊牌价 ￥499</span></a> \n &nbsp; &nbsp; &nbsp;</p></td><td width=\"50\"><br/></td></tr><tr><td width=\"455\" height=\"30\"><br/></td><td><br/></td><td width=\"50\"><br/></td></tr></tbody></table><p>模特佩戴配饰均为搭配不做销售用途。商品颜色请以实物为准，可参考平铺图颜色。</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/705/remark/c96b70883ab44c978abc268594b5b40b.jpg\" width=\"730\" height=\"1168\"/> \n &nbsp;</p><p>模特佩戴配饰均为搭配不做销售用途。商品颜色请以实物为准，可参考平铺图颜色。</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/705/remark/58d0cb67b1524b60ac6f2caa3f94e7e2.jpg\" width=\"610\" height=\"976\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/705/remark/126813aae5db4b769307bc382d5cdf67.jpg\"/> \n </p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/705/remark/9a225af2f0da40ac917a055f5040acc7.jpg\" width=\"730\" height=\"1046\"/> \n </p><p>本款更多颜色选择</p><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/705/remark/67722e84a81849b79a55f9b074c4eff9.jpg\" width=\"365\" height=\"584\"/></td><td><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/705/remark/69a7eeefbe9b47db89d7f1c548bcc176.jpg\" width=\"365\" height=\"584\"/></td></tr><tr style=\"text-align: center; line-height: 20px; font-size: 20px;\"><td style=\"padding-top: 30px; padding-bottom: 40px;\">黑色</td><td style=\"padding-top: 30px; padding-bottom: 40px;\">庆典红</td></tr></tbody></table><p>尺码 \n &nbsp;<span style=\"padding-left: 20px; font-size: 14px;\">因不同的计量方法，测量允许1-3cm内误差（CM）</span> \n </p><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">身高/净腰围/型号</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">肩宽</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">胸围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">腰围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">衣长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">袖长</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">155/76A/XS</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">56</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">104</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">66/68/53</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">46</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">160/80A/S</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">57</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">106</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">67/69/54</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">46.5</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">165/84A/M</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">59</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">110</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">69/71/56</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">47</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">170/88A/L</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">61</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">114</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">71/73/58</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">47.5</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/92A/XL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">63</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">118</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">72/74/59</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">48</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/96A/XXL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">65</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">122</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">73/75/60</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">48</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"line-height: 24px; font-size: 24px;\" width=\"120\">尺码参考</td><td width=\"44\"><br/></td><td width=\"175\">模特身高179</td><td width=\"91\">胸围82</td><td width=\"103\">臀围88</td><td width=\"98\">腰围60</td><td align=\"right\" width=\"99\">穿着M码</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"padding-top: 30px; border-top-color: rgb(204, 204, 204);\" width=\"120\" valign=\"top\"><p>设计卖点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>● \n &nbsp; &nbsp; &nbsp; &nbsp;<span style=\"padding-left: 12px;\">舒适面料 </span> \n &nbsp; &nbsp; &nbsp;</p><p>● \n &nbsp; &nbsp; &nbsp; &nbsp;<span style=\"padding-left: 12px;\">宽松版型</span> \n &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"padding-top: 30px; padding-bottom: 50px; border-top-color: rgb(204, 204, 204); border-bottom-color: rgb(204, 204, 204);\" width=\"120\" valign=\"top\"><p>商品指数</p></td><td style=\"padding-bottom: 50px; border-bottom-color: rgb(204, 204, 204);\" width=\"44\"><br/></td><td style=\"padding-top: 30px; padding-bottom: 50px; border-top-color: rgb(204, 204, 204); border-bottom-color: rgb(204, 204, 204);\" width=\"566\">薄厚指数轻薄薄适中厚加厚</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" background=\"http://ecphoto.bestseller.com.cn/onlyimages/2016_Q3/117124502E40_dp.jpg\" width=\"730\" height=\"800\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td width=\"455\" height=\"30\"><br/></td><td><br/></td><td width=\"50\"><br/></td></tr><tr><td width=\"455\"><br/></td><td bgcolor=\"#ffffff\"><p><span style=\"width: 225px; font-size: 20px; margin-bottom: 15px; display: block;\">成套搭配购买</span> \n &nbsp; &nbsp; &nbsp;</p><p><a style=\"color: rgb(0, 0, 0); text-decoration: none;\" href=\"https://item.jd.com/1587028479.html\" target=\"_blank\"><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/705/remark/ffebeb9bdc71456b963c0eecfbd3e249.jpg\" width=\"120\" height=\"73\"/> <span style=\"width: 225px; margin-top: 8px; display: block;\">吊牌价 ￥499</span></a> \n &nbsp; &nbsp; &nbsp;</p></td><td width=\"50\"><br/></td></tr><tr><td width=\"455\" height=\"30\"><br/></td><td><br/></td><td width=\"50\"><br/></td></tr></tbody></table><p>模特佩戴配饰均为搭配不做销售用途。商品颜色请以实物为准，可参考平铺图颜色。</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/705/remark/9c95cca023344176a977ce0340c894b8.jpg\" width=\"730\" height=\"1168\"/> \n &nbsp;</p><p>模特佩戴配饰均为搭配不做销售用途。商品颜色请以实物为准，可参考平铺图颜色。</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/705/remark/69a7eeefbe9b47db89d7f1c548bcc176.jpg\" width=\"610\" height=\"976\"/></p><p><br/></p>'),(0,706,NULL,'<p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/706/remark/33fc22c943a04bac94d4bb390eda4b20.jpg\"/> \n </p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/706/remark/93d8ddc7ddb54c698c162a008b3203b8.jpg\" width=\"730\" height=\"1046\"/> \n </p><p>本款更多颜色选择</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/706/remark/b2c237d2bf014b438d65fd8ae9a2c3a0.jpg\" width=\"730\"/> \n &nbsp;</p><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"\" width=\"146\">酒红色</td><td style=\"\" width=\"146\">黑色</td></tr></tbody></table><p>尺码 \n &nbsp; <span style=\"font-size:14px; padding-left:20px;\">单位cm</span> \n &nbsp;</p><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">身高/净腰围/型号</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">肩宽</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">胸围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">腰围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">衣长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">袖长</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">155/76A/XS</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">112</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">53</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">70</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">160/80A/S</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">114</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">54</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">71</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">165/84A/M</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">118</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">56</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">72</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">170/88A/L</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">122</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">58</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">73</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/92A/XL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">126</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">59</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">74</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/96A/XXL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">130</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">60</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">74</td></tr></tbody></table><p>因不同的计量方法，测量允许1-3cm内误差。</p><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"font-size:24px; line-height:24px;\" width=\"120\">尺码参考</td><td width=\"44\"><br/></td><td width=\"175\">模特身高177</td><td width=\"91\">胸围83</td><td width=\"103\">臀围87</td><td width=\"98\">腰围60</td><td align=\"right\" width=\"99\">穿着M码</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品信息</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>货号：11639R511</p><p>款名：FL8.5 FAN SWEAT(EDGE)</p><p>吊牌价：￥499</p><p>颜色：酒红色、黑色</p><p>大身面料：粘纤53%聚酯纤维41%氨纶6%（装饰部分除外）</p><p>袖子面料：粘纤52%聚酯纤维40%氨纶8%</p><p>里布：聚酯纤维95%氨纶5%</p><p>洗涤建议：30度水温下正常手洗，请避免含碱性的洗涤用品，反面洗涤、不宜用力拉伸，避免高温熨烫。</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>设计卖点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>● \n &nbsp; &nbsp; &nbsp; &nbsp; <span style=\"padding-left:12px;\">挺阔太空棉面料</span> \n &nbsp; &nbsp; &nbsp; &nbsp;</p><p>● \n &nbsp; &nbsp; &nbsp; &nbsp; <span style=\"padding-left:12px;\">宽松版型</span> \n &nbsp; &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品指数</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"566\">薄厚指数轻薄薄适中厚加厚</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>温馨提示</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p><span style=\"background-color:#ffe302;\">模特所佩戴饰品、配件均为搭配使用，不做销售用途</span> \n &nbsp; &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><p>本款优势</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/706/remark/dfdef75161b0456092a0a67300275aa5.jpg\" width=\"730\" height=\"730\"/> \n &nbsp;</p><p><span style=\"margin: 20px 0px 40px;\">螺纹底摆</span> \n &nbsp;</p><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/706/remark/dcd90cb27a2a40f2a35251850606b739.jpg\" width=\"365\" height=\"584\"/></td><td><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/706/remark/f9148c3f769e49f8a073633b379da990.jpg\" width=\"365\" height=\"584\"/></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td>简约暗扣</td><td>烫印图案装饰</td></tr></tbody></table><p><br/></p>',0,0,NULL,NULL,NULL,'<p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/706/remark/fcd263befcc04ce7a9c4964c31937341.jpg\"/> \n </p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/706/remark/a586c3b7b6894a11b347ecb3fa1e5628.jpg\" width=\"730\" height=\"1046\"/> \n </p><p>本款更多颜色选择</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/706/remark/7bf426f26b4047e5b7c804d3f77dc459.jpg\" width=\"730\"/> \n &nbsp;</p><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"\" width=\"146\">酒红色</td><td style=\"\" width=\"146\">黑色</td></tr></tbody></table><p>尺码 \n &nbsp; <span style=\"font-size:14px; padding-left:20px;\">单位cm</span> \n &nbsp;</p><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">身高/净腰围/型号</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">肩宽</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">胸围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">腰围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">衣长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">袖长</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">155/76A/XS</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">112</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">53</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">70</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">160/80A/S</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">114</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">54</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">71</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">165/84A/M</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">118</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">56</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">72</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">170/88A/L</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">122</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">58</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">73</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/92A/XL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">126</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">59</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">74</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/96A/XXL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">130</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">60</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">74</td></tr></tbody></table><p>因不同的计量方法，测量允许1-3cm内误差。</p><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"font-size:24px; line-height:24px;\" width=\"120\">尺码参考</td><td width=\"44\"><br/></td><td width=\"175\">模特身高177</td><td width=\"91\">胸围83</td><td width=\"103\">臀围87</td><td width=\"98\">腰围60</td><td align=\"right\" width=\"99\">穿着M码</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品信息</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>货号：11639R511</p><p>款名：FL8.5 FAN SWEAT(EDGE)</p><p>吊牌价：￥499</p><p>颜色：酒红色、黑色</p><p>大身面料：粘纤53%聚酯纤维41%氨纶6%（装饰部分除外）</p><p>袖子面料：粘纤52%聚酯纤维40%氨纶8%</p><p>里布：聚酯纤维95%氨纶5%</p><p>洗涤建议：30度水温下正常手洗，请避免含碱性的洗涤用品，反面洗涤、不宜用力拉伸，避免高温熨烫。</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>设计卖点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>● \n &nbsp; &nbsp; &nbsp; &nbsp; <span style=\"padding-left:12px;\">挺阔太空棉面料</span> \n &nbsp; &nbsp; &nbsp; &nbsp;</p><p>● \n &nbsp; &nbsp; &nbsp; &nbsp; <span style=\"padding-left:12px;\">宽松版型</span> \n &nbsp; &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品指数</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"566\">薄厚指数轻薄薄适中厚加厚</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>温馨提示</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p><span style=\"background-color:#ffe302;\">模特所佩戴饰品、配件均为搭配使用，不做销售用途</span> \n &nbsp; &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><p>本款优势</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/706/remark/82f1fae1be8247f7a5727aee1798a1ea.jpg\" width=\"730\" height=\"730\"/> \n &nbsp;</p><p><span style=\"margin: 20px 0px 40px;\">螺纹底摆</span> \n &nbsp;</p><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/706/remark/f55ceedfc2554994bedc4b32634e2db5.jpg\" width=\"365\" height=\"584\"/></td><td><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/706/remark/abdabdf986134b0dabe18f69d776e06f.jpg\" width=\"365\" height=\"584\"/></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td>简约暗扣</td><td>烫印图案装饰</td></tr></tbody></table><p><br/></p>'),(0,707,NULL,'<p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/707/remark/370abcf671fa4e7799470ad6602ad626.jpg\"/> \n </p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/707/remark/14879192a85042e090b97f912950bce4.jpg\" width=\"730\" height=\"1046\"/> \n </p><p>本款更多颜色选择</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/707/remark/0a823f368fa4454eb1e652bbfad47a4f.jpg\" width=\"730\"/> \n &nbsp;</p><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"\" width=\"146\">樱桃红</td><td style=\"\" width=\"146\">黑</td><td style=\"\" width=\"146\">奶白</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>搭配特点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\">樱桃红—经典优雅黑—显瘦百搭奶白—清新条纹</td></tr></tbody></table><p>尺码 \n &nbsp;<span style=\"font-size:14px; padding-left:20px;\">单位cm</span> \n </p><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">身高/净腰围/型号</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">衣长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">胸围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">腰围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">袖长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">肩宽</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">臀围</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">155/76A/XS</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">95</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">72</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">64</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">33.6</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">39.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">79</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">160/80A/S</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">96</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">74</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">66</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">34.3</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">40</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">81</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">165/84A/M</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">98</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">78</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">70</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">35</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">41</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">85</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">170/88A/L</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">100</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">82</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">74</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">35.7</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">42</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">89</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/92A/XL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">101</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">86</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">78</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">36.4</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">43</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">93</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/96A/XXL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">102</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">90</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">82</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">36.4</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">44</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">97</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"font-size:24px; line-height:24px;\" width=\"120\">尺码参考</td><td width=\"44\"><br/></td><td width=\"175\">模特身高177</td><td width=\"91\">胸围83</td><td width=\"103\">臀围87</td><td width=\"98\">腰围60</td><td align=\"right\" width=\"99\">穿着M码</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品信息</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>货号：116361504</p><p>款名：P MEGGAN FITTED JERSEY DRESS (ESSENTIAL)</p><p>吊牌价：￥349</p><p>颜色：樱桃红、黑、奶白</p><p>面料：锦纶93% 氨纶7%</p><p>洗涤建议：30度水温下正常手洗，请避免含碱性的洗涤用品，反面洗涤、不宜用力拉伸，避免高温熨烫</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>设计卖点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>● \n &nbsp; &nbsp; &nbsp; &nbsp;<span style=\"padding-left:12px;\">舒适含莱卡面料</span> \n &nbsp; &nbsp; &nbsp;</p><p>● \n &nbsp; &nbsp; &nbsp; &nbsp;<span style=\"padding-left:12px;\">修身款型</span> \n &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品指数</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"566\">薄厚指数轻薄薄适中厚实弹性指数无弹微弹适中高弹</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>温馨提示</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p><span style=\"background-color:#ffe302;\">请用低温手洗，轻轻拧去水分，晾在通风处阴干，切勿在阳光中暴晒</span> \n &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><p>本款优势</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/707/remark/af81b23b52e54c58b8e7456bd60686ac.jpg\" width=\"730\" height=\"730\"/></p>',0,0,NULL,NULL,NULL,'<p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/707/remark/c65362bdde53431fb00b9ff140899d05.jpg\"/> \n </p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/707/remark/a6bab3abd0204e95ac57226bdc019c4e.jpg\" width=\"730\" height=\"1046\"/> \n </p><p>本款更多颜色选择</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/707/remark/0cdfa7cac3e54484a0e6fd226d7a9ee2.jpg\" width=\"730\"/> \n &nbsp;</p><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"\" width=\"146\">樱桃红</td><td style=\"\" width=\"146\">黑</td><td style=\"\" width=\"146\">奶白</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>搭配特点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\">樱桃红—经典优雅黑—显瘦百搭奶白—清新条纹</td></tr></tbody></table><p>尺码 \n &nbsp;<span style=\"font-size:14px; padding-left:20px;\">单位cm</span> \n </p><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">身高/净腰围/型号</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">衣长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">胸围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">腰围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">袖长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">肩宽</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">臀围</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">155/76A/XS</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">95</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">72</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">64</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">33.6</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">39.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">79</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">160/80A/S</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">96</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">74</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">66</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">34.3</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">40</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">81</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">165/84A/M</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">98</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">78</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">70</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">35</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">41</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">85</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">170/88A/L</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">100</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">82</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">74</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">35.7</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">42</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">89</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/92A/XL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">101</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">86</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">78</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">36.4</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">43</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">93</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/96A/XXL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">102</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">90</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">82</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">36.4</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">44</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">97</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"font-size:24px; line-height:24px;\" width=\"120\">尺码参考</td><td width=\"44\"><br/></td><td width=\"175\">模特身高177</td><td width=\"91\">胸围83</td><td width=\"103\">臀围87</td><td width=\"98\">腰围60</td><td align=\"right\" width=\"99\">穿着M码</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品信息</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>货号：116361504</p><p>款名：P MEGGAN FITTED JERSEY DRESS (ESSENTIAL)</p><p>吊牌价：￥349</p><p>颜色：樱桃红、黑、奶白</p><p>面料：锦纶93% 氨纶7%</p><p>洗涤建议：30度水温下正常手洗，请避免含碱性的洗涤用品，反面洗涤、不宜用力拉伸，避免高温熨烫</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>设计卖点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>● \n &nbsp; &nbsp; &nbsp; &nbsp;<span style=\"padding-left:12px;\">舒适含莱卡面料</span> \n &nbsp; &nbsp; &nbsp;</p><p>● \n &nbsp; &nbsp; &nbsp; &nbsp;<span style=\"padding-left:12px;\">修身款型</span> \n &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品指数</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"566\">薄厚指数轻薄薄适中厚实弹性指数无弹微弹适中高弹</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>温馨提示</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p><span style=\"background-color:#ffe302;\">请用低温手洗，轻轻拧去水分，晾在通风处阴干，切勿在阳光中暴晒</span> \n &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><p>本款优势</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/707/remark/99bd8aaf130843289a9cc9eb6c807e3f.jpg\" width=\"730\" height=\"730\"/></p>'),(0,708,NULL,'<p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/708/remark/1e814706dad24527a6c39fb472415918.jpg\"/></p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/708/remark/57c2869b176e4711a651a2ce37f87487.jpg\" width=\"730\" height=\"1046\"/></p><p>本款更多颜色选择</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/708/remark/74457213cc6944b6a05807788c380e8c.jpg\" width=\"730\"/></p><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"\" width=\"146\">黑色</td><td style=\"\" width=\"146\">奶白</td><td style=\"\" width=\"146\">深红</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>搭配特点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\">黑色—增添神秘酷感味道奶白—简约自然清新感觉深红—洋溢浪漫活力</td></tr></tbody></table><p>尺码 &nbsp; <span style=\"font-size:14px; padding-left:20px;\">单位cm</span></p><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">身高/净腰围/型号</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">衣长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">胸围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">腰围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">袖长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">肩宽</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">臀围</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">155/76A/XS</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">87</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">86</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">61/84</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">43</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">36.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">160/80A/S</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">88</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">88</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">63/86</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">44</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">37</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">165/84A/M</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">90</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">92</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">67/90</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">45</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">38</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">170/88A/L</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">92</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">96</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">71/94</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">46</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">39</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/92A/XL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">93</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">100</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">75/98</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">47</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">40</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/96A/XXL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">94</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">104</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">79/102</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">47</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">41</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"font-size:24px; line-height:24px;\" width=\"120\">尺码参考</td><td width=\"44\"><br/></td><td width=\"175\">模特身高179</td><td width=\"91\">胸围82</td><td width=\"103\">臀围88</td><td width=\"98\">腰围60</td><td align=\"right\" width=\"99\">穿着M码</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品信息</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>货号：116307501</p><p>款名：P GILL A LINE DRESS(LOVE)</p><p>吊牌价：￥899</p><p>颜色：深红、黑色、奶白</p><p>绣花面料：棉100%</p><p>里料：棉100%</p><p>洗涤建议：30度水温下正常手洗，请避免含碱性的洗涤用品，反面洗涤、不宜用力拉伸，避免高温熨烫。</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>设计卖点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>● &nbsp; &nbsp; &nbsp; &nbsp; <span style=\"padding-left:12px;\"></span>松紧收腰设计</p><p>● &nbsp; &nbsp; &nbsp; &nbsp; <span style=\"padding-left:12px;\"></span>镂空蕾丝设计</p><p>● &nbsp; &nbsp; &nbsp; &nbsp; <span style=\"padding-left:12px;\">背面镂空水滴扣</span></p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品指数</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"566\">薄厚指数轻薄薄适中厚实弹性指数无弹微弹适中高弹</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>温馨提示</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p><span style=\"background-color:#ffe302;\">请用低温手洗，轻轻拧去水分，晾在通风处阴干，切勿在阳光中暴晒。</span></p></td></tr></tbody></table>',0,0,NULL,NULL,NULL,'<p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/708/remark/a9600b96df7c43f8a8950c904eac618f.jpg\"/></p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/708/remark/3ca901c79f4048e3ba1579fc0e384bd3.jpg\" width=\"730\" height=\"1046\"/></p><p>本款更多颜色选择</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/708/remark/b8907c3d2ece4f9fabccfa6f85461c03.jpg\" width=\"730\"/></p><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"\" width=\"146\">黑色</td><td style=\"\" width=\"146\">奶白</td><td style=\"\" width=\"146\">深红</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>搭配特点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\">黑色—增添神秘酷感味道奶白—简约自然清新感觉深红—洋溢浪漫活力</td></tr></tbody></table><p>尺码 &nbsp; <span style=\"font-size:14px; padding-left:20px;\">单位cm</span></p><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">身高/净腰围/型号</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">衣长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">胸围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">腰围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">袖长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">肩宽</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">臀围</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">155/76A/XS</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">87</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">86</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">61/84</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">43</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">36.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">160/80A/S</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">88</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">88</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">63/86</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">44</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">37</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">165/84A/M</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">90</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">92</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">67/90</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">45</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">38</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">170/88A/L</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">92</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">96</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">71/94</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">46</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">39</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/92A/XL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">93</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">100</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">75/98</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">47</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">40</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/96A/XXL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">94</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">104</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">79/102</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">47</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">41</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"font-size:24px; line-height:24px;\" width=\"120\">尺码参考</td><td width=\"44\"><br/></td><td width=\"175\">模特身高179</td><td width=\"91\">胸围82</td><td width=\"103\">臀围88</td><td width=\"98\">腰围60</td><td align=\"right\" width=\"99\">穿着M码</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品信息</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>货号：116307501</p><p>款名：P GILL A LINE DRESS(LOVE)</p><p>吊牌价：￥899</p><p>颜色：深红、黑色、奶白</p><p>绣花面料：棉100%</p><p>里料：棉100%</p><p>洗涤建议：30度水温下正常手洗，请避免含碱性的洗涤用品，反面洗涤、不宜用力拉伸，避免高温熨烫。</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>设计卖点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>● &nbsp; &nbsp; &nbsp; &nbsp; <span style=\"padding-left:12px;\"></span>松紧收腰设计</p><p>● &nbsp; &nbsp; &nbsp; &nbsp; <span style=\"padding-left:12px;\"></span>镂空蕾丝设计</p><p>● &nbsp; &nbsp; &nbsp; &nbsp; <span style=\"padding-left:12px;\">背面镂空水滴扣</span></p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品指数</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"566\">薄厚指数轻薄薄适中厚实弹性指数无弹微弹适中高弹</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>温馨提示</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p><span style=\"background-color:#ffe302;\">请用低温手洗，轻轻拧去水分，晾在通风处阴干，切勿在阳光中暴晒。</span></p></td></tr></tbody></table>'),(0,709,'发布的商品不符合要求','<p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/709/remark/e53452208c4a4f2fb2edfcf6249415e1.jpg\"/> \n </p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/709/remark/a73d4beeff1c4c02b4d845e87f86f018.jpg\" width=\"730\" height=\"1046\"/> \n </p><p>尺码 \n &nbsp;<span style=\"font-size:14px; padding-left:20px;\">单位cm</span> \n </p><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td rowspan=\"7\" style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"105\">针织卫衣</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"165\">身高/净腰围/型号</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"73\">衣长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"73\">胸围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"73\">腰围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"74\">袖长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"73\">肩宽</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"76\">臀围</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">155/76A/XS</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">73</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">108</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">24</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">61</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">160/80A/S</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">74</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">110</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">24.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">62</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">165/84A/M</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">76</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">114</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">25</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">64</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">170/88A/L</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">78</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">118</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">25.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">66</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/92A/XL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">79</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">122</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">26</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">68</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/96A/XXL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">80</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">126</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">26</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">70</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td rowspan=\"7\" style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"105\">衬衫</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"165\">身高/净腰围/型号</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"73\">衣长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"73\">胸围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"73\">腰围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"73\">袖长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"74\">肩宽</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"76\">臀围</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">155/76A/XS</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">102</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">92</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">49</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">37</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">160/80A/S</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">103</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">94</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">50</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">37.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">165/84A/M</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">105</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">98</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">51</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">38.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">170/88A/L</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">107</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">102</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">52</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">39.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/92A/XL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">108</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">106</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">53</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">40.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/96A/XXL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">109</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">110</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">53</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">41.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr></tbody></table><p>因不同的计量方法，测量允许1-3cm内误差。</p><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\"><tbody><tr class=\"firstRow\"></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\"><tbody><tr class=\"firstRow\"></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\"><tbody><tr class=\"firstRow\"><td style=\"font-size:24px; line-height:24px;\" width=\"120\">尺码参考</td><td width=\"44\"><br/></td><td width=\"175\">模特身高177</td><td width=\"91\">胸围83</td><td width=\"103\">臀围87</td><td width=\"98\">腰围60</td><td align=\"right\" width=\"99\">穿着M码</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品信息</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>货号：116360504</p><p>款名：FL8.5 FANCY SHIRT SWEAT</p><p>吊牌价：¥599</p><p>颜色：花灰色</p><p>外层大身面料：棉100%</p><p>里层面料：棉100%</p><p>洗涤建议：30度水温下正常手洗，请避免含碱性的洗涤用品，反面洗涤、不宜用力拉伸，避免高温熨烫</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>设计卖点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>● \n &nbsp; &nbsp; &nbsp; &nbsp;<span style=\"padding-left:12px;\">纯棉舒适面料</span> \n &nbsp; &nbsp; &nbsp;</p><p>● \n &nbsp; &nbsp; &nbsp; &nbsp;<span style=\"padding-left:12px;\">两件套叠搭设计</span> \n &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品指数</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"566\">薄厚指数轻薄薄适中厚实弹性指数无弹微弹适中高弹</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>温馨提示</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p><span style=\"background-color:#ffe302;\">请用低温手洗，轻轻拧去水分，晾在通风处阴干，切勿在阳光中暴晒</span> \n &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><p>本款优势</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/709/remark/30a6b81bf0e342a1a88c2a70e3be9aa0.jpg\" width=\"730\" height=\"730\"/> \n </p><p>螺纹圆领设计</p><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\"><tbody><tr class=\"firstRow\"><td><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/709/remark/58ed8ed6e07e43d1ab48949f2dd61369.jpg\" width=\"365\" height=\"584\"/></td><td><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/709/remark/3c7e51ccc2f24c838364e3d7397c0168.jpg\" width=\"365\" height=\"584\"/></td></tr></tbody></table>',0,0,NULL,NULL,NULL,'<p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/709/remark/1f4be3ef4e594b56abc963d72c3cc2f3.jpg\"/> \n </p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/709/remark/7bd2a7dfdd254f3096a7e4be8e74a5cc.jpg\" width=\"730\" height=\"1046\"/> \n </p><p>尺码 \n &nbsp;<span style=\"font-size:14px; padding-left:20px;\">单位cm</span> \n </p><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td rowspan=\"7\" style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"105\">针织卫衣</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"165\">身高/净腰围/型号</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"73\">衣长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"73\">胸围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"73\">腰围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"74\">袖长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"73\">肩宽</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"76\">臀围</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">155/76A/XS</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">73</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">108</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">24</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">61</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">160/80A/S</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">74</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">110</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">24.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">62</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">165/84A/M</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">76</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">114</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">25</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">64</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">170/88A/L</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">78</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">118</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">25.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">66</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/92A/XL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">79</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">122</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">26</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">68</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/96A/XXL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">80</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">126</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">26</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">70</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td rowspan=\"7\" style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"105\">衬衫</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"165\">身高/净腰围/型号</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"73\">衣长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"73\">胸围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"73\">腰围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"73\">袖长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"74\">肩宽</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\" width=\"76\">臀围</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">155/76A/XS</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">102</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">92</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">49</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">37</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">160/80A/S</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">103</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">94</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">50</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">37.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">165/84A/M</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">105</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">98</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">51</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">38.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">170/88A/L</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">107</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">102</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">52</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">39.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/92A/XL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">108</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">106</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">53</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">40.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/96A/XXL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">109</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">110</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">53</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">41.5</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr></tbody></table><p>因不同的计量方法，测量允许1-3cm内误差。</p><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\"><tbody><tr class=\"firstRow\"></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\"><tbody><tr class=\"firstRow\"></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\"><tbody><tr class=\"firstRow\"><td style=\"font-size:24px; line-height:24px;\" width=\"120\">尺码参考</td><td width=\"44\"><br/></td><td width=\"175\">模特身高177</td><td width=\"91\">胸围83</td><td width=\"103\">臀围87</td><td width=\"98\">腰围60</td><td align=\"right\" width=\"99\">穿着M码</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品信息</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>货号：116360504</p><p>款名：FL8.5 FANCY SHIRT SWEAT</p><p>吊牌价：¥599</p><p>颜色：花灰色</p><p>外层大身面料：棉100%</p><p>里层面料：棉100%</p><p>洗涤建议：30度水温下正常手洗，请避免含碱性的洗涤用品，反面洗涤、不宜用力拉伸，避免高温熨烫</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>设计卖点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>● \n &nbsp; &nbsp; &nbsp; &nbsp;<span style=\"padding-left:12px;\">纯棉舒适面料</span> \n &nbsp; &nbsp; &nbsp;</p><p>● \n &nbsp; &nbsp; &nbsp; &nbsp;<span style=\"padding-left:12px;\">两件套叠搭设计</span> \n &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品指数</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"566\">薄厚指数轻薄薄适中厚实弹性指数无弹微弹适中高弹</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>温馨提示</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p><span style=\"background-color:#ffe302;\">请用低温手洗，轻轻拧去水分，晾在通风处阴干，切勿在阳光中暴晒</span> \n &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><p>本款优势</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/709/remark/19e8b9085d2949ee8c77d7d77e7ca47a.jpg\" width=\"730\" height=\"730\"/> \n </p><p>螺纹圆领设计</p><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\"><tbody><tr class=\"firstRow\"><td><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/709/remark/00d960196ae645c289063eb3e44b7254.jpg\" width=\"365\" height=\"584\"/></td><td><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/709/remark/2fe153110e2242c9939c5b1dc0b34200.jpg\" width=\"365\" height=\"584\"/></td></tr></tbody></table>'),(0,710,'发布的商品不符合要求','<p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/710/remark/61fa364bf0b34f538f1dd6eea62aae2c.jpg\"/> \n </p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/710/remark/4c214253c0cf4392a4f288c7754d2ef1.jpg\" width=\"730\" height=\"1046\"/> \n </p><p>尺码 \n &nbsp;<span style=\"font-size:14px; padding-left:20px;\">单位cm</span> \n </p><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">身高/净腰围/型号</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">衣长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">胸围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">腰围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">袖长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">肩宽</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">臀围</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">155/76A/XS</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">37</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">69</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">160/80A/S</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">38</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">71</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">165/84A/M</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">40</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">75</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">170/88A/L</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">42</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">79</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/92A/XL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">43</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">83</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/96A/XXL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">44</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">87</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr></tbody></table><p>因不同的计量方法，测量允许1-3cm内误差。</p><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"font-size:24px; line-height:24px;\" width=\"120\">尺码参考</td><td width=\"44\"><br/></td><td width=\"175\">模特身高179</td><td width=\"91\">胸围82</td><td width=\"103\">臀围88</td><td width=\"98\">腰围60</td><td align=\"right\" width=\"99\">穿着M码</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品信息</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>货号：116342524</p><p>款名：FL8 RINO DENIM OVERALL DRESS</p><p>吊牌价：￥499</p><p>颜色：390洗水牛仔蓝</p><p>面料：棉100%</p><p>口袋布：聚酯纤维72%棉28%</p><p>洗涤建议：请单独反底洗涤，不可甩干。牛仔裤在首次洗涤时用食盐和醋浸泡后保色效果较好。</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>设计卖点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>● \n &nbsp; &nbsp; &nbsp; &nbsp;<span style=\"padding-left:12px;\">纯棉面料</span> \n &nbsp; &nbsp; &nbsp;</p><p>● \n &nbsp; &nbsp; &nbsp; &nbsp;<span style=\"padding-left:12px;\">宽松版型</span> \n &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品指数</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"566\">薄厚指数轻薄薄适中厚实弹性指数无弹微弹适中高弹</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>温馨提示</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p><span style=\"background-color:#ffe302;\">模特所佩戴饰品、配件均为搭配使用，不做销售用途</span> \n &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><p><br/></p>',0,0,NULL,NULL,NULL,'<p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/710/remark/5283821cd69e44e9a1c21559c76a5cff.jpg\"/> \n </p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/710/remark/360daa78654f4ba6a2c247b507c11d03.jpg\" width=\"730\" height=\"1046\"/> \n </p><p>尺码 \n &nbsp;<span style=\"font-size:14px; padding-left:20px;\">单位cm</span> \n </p><table cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">身高/净腰围/型号</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">衣长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">胸围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">腰围</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">袖长</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">肩宽</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">臀围</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">155/76A/XS</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">37</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">69</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">160/80A/S</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">38</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">71</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">165/84A/M</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">40</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">75</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">170/88A/L</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">42</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">79</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/92A/XL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">43</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">83</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr><tr><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">175/96A/XXL</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">44</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">87</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td><td style=\"border-color: rgb(241, 241, 241);\" align=\"center\">/</td></tr></tbody></table><p>因不同的计量方法，测量允许1-3cm内误差。</p><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"font-size:24px; line-height:24px;\" width=\"120\">尺码参考</td><td width=\"44\"><br/></td><td width=\"175\">模特身高179</td><td width=\"91\">胸围82</td><td width=\"103\">臀围88</td><td width=\"98\">腰围60</td><td align=\"right\" width=\"99\">穿着M码</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品信息</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>货号：116342524</p><p>款名：FL8 RINO DENIM OVERALL DRESS</p><p>吊牌价：￥499</p><p>颜色：390洗水牛仔蓝</p><p>面料：棉100%</p><p>口袋布：聚酯纤维72%棉28%</p><p>洗涤建议：请单独反底洗涤，不可甩干。牛仔裤在首次洗涤时用食盐和醋浸泡后保色效果较好。</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>设计卖点</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p>● \n &nbsp; &nbsp; &nbsp; &nbsp;<span style=\"padding-left:12px;\">纯棉面料</span> \n &nbsp; &nbsp; &nbsp;</p><p>● \n &nbsp; &nbsp; &nbsp; &nbsp;<span style=\"padding-left:12px;\">宽松版型</span> \n &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>商品指数</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"566\">薄厚指数轻薄薄适中厚实弹性指数无弹微弹适中高弹</td></tr></tbody></table><table cellspacing=\"0\" cellpadding=\"0\" width=\"730\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"border-top-color: rgb(204, 204, 204); padding-top: 30px;\" width=\"120\" valign=\"top\"><p>温馨提示</p></td><td width=\"44\"><br/></td><td style=\"border-top-color: rgb(204, 204, 204);\" width=\"566\"><p><span style=\"background-color:#ffe302;\">模特所佩戴饰品、配件均为搭配使用，不做销售用途</span> \n &nbsp; &nbsp; &nbsp;</p></td></tr></tbody></table><p><br/></p>'),(0,711,NULL,'<p>吊带连衣裙，迷你裙长，甜美花朵印花图案，衣身抽褶设计，系带肩带，斜插前口袋。</p><p>细节：</p><p>- 面料：100%粘性纤维</p><p>- 冷水机洗</p><p>-BY FOREVER21</p><p>&nbsp;</p><p>&nbsp;</p><p><br/></p><table cellspacing=\"0\" cellpadding=\"0\" width=\"670\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 透明度 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/ac36055642a84e618d4fe68fb9151418.gif\" align=\"absmiddle\"/> 有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/7ed73a425bb24a52a302bd69be6d4d87.gif\" align=\"absmiddle\"/> 稍微有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/ac36055642a84e618d4fe68fb9151418.gif\" align=\"absmiddle\"/> 没有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 厚度感 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/ac36055642a84e618d4fe68fb9151418.gif\" align=\"absmiddle\"/> &nbsp;厚</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/ac36055642a84e618d4fe68fb9151418.gif\" align=\"absmiddle\"/> 中等</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/7ed73a425bb24a52a302bd69be6d4d87.gif\" align=\"absmiddle\"/> 薄</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 内衬 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/ac36055642a84e618d4fe68fb9151418.gif\" align=\"absmiddle\"/> 有内衬</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/7ed73a425bb24a52a302bd69be6d4d87.gif\" align=\"absmiddle\"/> 没有内衬</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 弹力性 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/ac36055642a84e618d4fe68fb9151418.gif\" align=\"absmiddle\"/> 有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/ac36055642a84e618d4fe68fb9151418.gif\" align=\"absmiddle\"/> 稍微有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/7ed73a425bb24a52a302bd69be6d4d87.gif\" align=\"absmiddle\"/> 没有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 洗涤方法 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/ac36055642a84e618d4fe68fb9151418.gif\" align=\"absmiddle\"/> 手洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/7ed73a425bb24a52a302bd69be6d4d87.gif\" align=\"absmiddle\"/> 用洗衣机洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/7ed73a425bb24a52a302bd69be6d4d87.gif\" align=\"absmiddle\"/> 凉水洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/ac36055642a84e618d4fe68fb9151418.gif\" align=\"absmiddle\"/> 中性洗衣粉洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/ac36055642a84e618d4fe68fb9151418.gif\" align=\"absmiddle\"/> 反面干</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/ac36055642a84e618d4fe68fb9151418.gif\" align=\"absmiddle\"/> 干洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/ac36055642a84e618d4fe68fb9151418.gif\" align=\"absmiddle\"/> 湿巾擦</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr></tbody></table><p><br/></p><p><strong><span style=\"color:#FF0000\">Y</span></strong> \n &nbsp;</p><p><br/></p><p><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/5e3cb05ac49f444cb4d6a2b138dab099.gif\" height=\"27\" width=\"750\"/> \n </p><p><br/> \n &nbsp;<img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/f6137916e66f4535a629a7bbf6b6c677.jpg\"/> \n &nbsp;<br/> &nbsp;\n &nbsp;<br/> &nbsp;\n &nbsp;<br/> \n &nbsp;<img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/672289f6c88a474db93d64ffe5db31d6.jpg\"/> \n &nbsp;<br/> \n </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/4f534989375e47bbbf849765d9019757.gif\" height=\"20\" width=\"750\"/> \n &nbsp;</p><center><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/66eb7c92fb324ac1a21a561e3d19f4c4.jpg\"/> \n &nbsp;</center><p><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/3ac038f5700f4591b127d63bbecf1295.jpg\"/> \n </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/97fd9465e71f48fbabd20cf0508c3e62.gif\" height=\"27\" width=\"750\"/> \n </p><p><span style=\"color:#000000\"><strong><a span=\"\" style=\"margin-left:5px;\">商品详细尺码</a></strong>(本商品的实际尺码) :</span> \n &nbsp;</p><table height=\"100%\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"85%\"><tbody><tr class=\"firstRow\"><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><p><span style=\"color:#000000\"><strong>商品详细尺码</strong>(本商品的实际尺码) :</span> </p>尺码实测位子SML肩宽（Cm)---领型宽（Cm)---领型深（Cm)---胸围(胸单面x2)（Cm)566064 腰围(腰单面x2)（Cm)---衣长（Cm)626364</td></tr></tbody></table><p><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/1406bf83f1d848c69ca272bbd15a670c.gif\" height=\"27\" width=\"750\"/> \n </p><center><table cellspacing=\"0\" cellpadding=\"0\" width=\"600\"><tbody><tr class=\"firstRow\"><td colspan=\"3\"><br/></td></tr><tr><td colspan=\"3\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/e45b3bad4c9d4e558856b798117b462f.gif\"/></td></tr></tbody></table></center><p><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/d805a97045d44706a091691c89b7e8f4.jpg\"/> \n </p><p><span style=\"color:red\"><strong>商品只接受退货(饰品及内衣类商品无法退货),无法换货及修理,如申请换货及修理均按退货处理.</strong></span> </p><p><br/> </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/73394a2abe0c4dff9eef7bfb6796ac45.jpg\" height=\"417\" width=\"750\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p>吊带连衣裙，迷你裙长，甜美花朵印花图案，衣身抽褶设计，系带肩带，斜插前口袋。</p><p>细节：</p><p>- 面料：100%粘性纤维</p><p>- 冷水机洗</p><p>-BY FOREVER21</p><p>&nbsp;</p><p>&nbsp;</p><p><br/></p><table cellspacing=\"0\" cellpadding=\"0\" width=\"670\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 透明度 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/81a7042bf4b340d1984f35fb582867f3.gif\" align=\"absmiddle\"/> 有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/e96bd2d55488489d95c24c3848dd7d81.gif\" align=\"absmiddle\"/> 稍微有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/81a7042bf4b340d1984f35fb582867f3.gif\" align=\"absmiddle\"/> 没有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 厚度感 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/81a7042bf4b340d1984f35fb582867f3.gif\" align=\"absmiddle\"/> &nbsp;厚</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/81a7042bf4b340d1984f35fb582867f3.gif\" align=\"absmiddle\"/> 中等</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/e96bd2d55488489d95c24c3848dd7d81.gif\" align=\"absmiddle\"/> 薄</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 内衬 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/81a7042bf4b340d1984f35fb582867f3.gif\" align=\"absmiddle\"/> 有内衬</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/e96bd2d55488489d95c24c3848dd7d81.gif\" align=\"absmiddle\"/> 没有内衬</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 弹力性 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/81a7042bf4b340d1984f35fb582867f3.gif\" align=\"absmiddle\"/> 有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/81a7042bf4b340d1984f35fb582867f3.gif\" align=\"absmiddle\"/> 稍微有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/e96bd2d55488489d95c24c3848dd7d81.gif\" align=\"absmiddle\"/> 没有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 洗涤方法 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/81a7042bf4b340d1984f35fb582867f3.gif\" align=\"absmiddle\"/> 手洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/e96bd2d55488489d95c24c3848dd7d81.gif\" align=\"absmiddle\"/> 用洗衣机洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/e96bd2d55488489d95c24c3848dd7d81.gif\" align=\"absmiddle\"/> 凉水洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/81a7042bf4b340d1984f35fb582867f3.gif\" align=\"absmiddle\"/> 中性洗衣粉洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/81a7042bf4b340d1984f35fb582867f3.gif\" align=\"absmiddle\"/> 反面干</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/81a7042bf4b340d1984f35fb582867f3.gif\" align=\"absmiddle\"/> 干洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/81a7042bf4b340d1984f35fb582867f3.gif\" align=\"absmiddle\"/> 湿巾擦</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr></tbody></table><p><br/></p><p><strong><span style=\"color:#FF0000\">Y</span></strong> \n &nbsp;</p><p><br/></p><p><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/1b01679c738a4732860cd4efb725b569.gif\" height=\"27\" width=\"750\"/> \n </p><p><br/> \n &nbsp;<img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/46a281d34e34464381a9b27a78dbebeb.jpg\"/> \n &nbsp;<br/> &nbsp;\n &nbsp;<br/> &nbsp;\n &nbsp;<br/> \n &nbsp;<img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/bd26d1d63dc640759168331aa2b9ed17.jpg\"/> \n &nbsp;<br/> \n </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/cecf21256a9644a8a1dbccadf0a247c3.gif\" height=\"20\" width=\"750\"/> \n &nbsp;</p><center><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/5d1889a7314f40ea89c469d6413ef978.jpg\"/> \n &nbsp;</center><p><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/d09f73a6455249d389224917dc856f78.jpg\"/> \n </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/e3894e6d8d2d409dbb8cb7afc0867e2d.gif\" height=\"27\" width=\"750\"/> \n </p><p><span style=\"color:#000000\"><strong><a span=\"\" style=\"margin-left:5px;\">商品详细尺码</a></strong>(本商品的实际尺码) :</span> \n &nbsp;</p><table height=\"100%\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"85%\"><tbody><tr class=\"firstRow\"><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><p><span style=\"color:#000000\"><strong>商品详细尺码</strong>(本商品的实际尺码) :</span> </p>尺码实测位子SML肩宽（Cm)---领型宽（Cm)---领型深（Cm)---胸围(胸单面x2)（Cm)566064 腰围(腰单面x2)（Cm)---衣长（Cm)626364</td></tr></tbody></table><p><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/bbfb17ce2d834b88827dc40b9d716720.gif\" height=\"27\" width=\"750\"/> \n </p><center><table cellspacing=\"0\" cellpadding=\"0\" width=\"600\"><tbody><tr class=\"firstRow\"><td colspan=\"3\"><br/></td></tr><tr><td colspan=\"3\"><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/88ed0d72d4044384a88349913eadab1b.gif\"/></td></tr></tbody></table></center><p><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/8072ead198984468839a31b418f6c00a.jpg\"/> \n </p><p><span style=\"color:red\"><strong>商品只接受退货(饰品及内衣类商品无法退货),无法换货及修理,如申请换货及修理均按退货处理.</strong></span> </p><p><br/> </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/711/remark/d97a92d4cd9040f787a827beb343488e.jpg\" height=\"417\" width=\"750\"/></p><p><br/></p>'),(0,712,NULL,'<p><strong><span style=\"color:#000000\">商品说明 :</span></strong><br/> </p><p>&nbsp;</p><p>短袖连衣裙，及膝款式，纽扣门襟，露肩吊带款式，肩带可调节，侧边隐形拉链开合，质地轻盈。</p><p>细节：</p><p>- 面料：100%粘性纤维；衬里：100%聚酯纤维</p><p>- 冷水机洗</p><p>- 模特信息：身高176cm，身穿尺码S</p><p>Forever21 Contemporary的尺码偏大，比Forever21的标准尺码要大一码，建议您选购Forever21 Contemporary的服装时小一号尺码。</p><p>- BY FOREVER21</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p><br/></p><table cellspacing=\"0\" cellpadding=\"0\" width=\"670\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 透明度 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/2d00d5a4469d4a418312cad464f010d4.gif\" align=\"absmiddle\"/> 有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/2d00d5a4469d4a418312cad464f010d4.gif\" align=\"absmiddle\"/> 稍微有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/a9db1ee24109401d8108564fa6295dfe.gif\" align=\"absmiddle\"/> 没有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 厚度感 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/2d00d5a4469d4a418312cad464f010d4.gif\" align=\"absmiddle\"/> &nbsp;厚</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/2d00d5a4469d4a418312cad464f010d4.gif\" align=\"absmiddle\"/> 中等</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/a9db1ee24109401d8108564fa6295dfe.gif\" align=\"absmiddle\"/> 薄</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 内衬 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/a9db1ee24109401d8108564fa6295dfe.gif\" align=\"absmiddle\"/> 有内衬</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/2d00d5a4469d4a418312cad464f010d4.gif\" align=\"absmiddle\"/> 没有内衬</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 弹力性 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/2d00d5a4469d4a418312cad464f010d4.gif\" align=\"absmiddle\"/> 有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/2d00d5a4469d4a418312cad464f010d4.gif\" align=\"absmiddle\"/> 稍微有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/a9db1ee24109401d8108564fa6295dfe.gif\" align=\"absmiddle\"/> 没有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 洗涤方法 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/2d00d5a4469d4a418312cad464f010d4.gif\" align=\"absmiddle\"/> 手洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/a9db1ee24109401d8108564fa6295dfe.gif\" align=\"absmiddle\"/> 用洗衣机洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/a9db1ee24109401d8108564fa6295dfe.gif\" align=\"absmiddle\"/> 凉水洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/2d00d5a4469d4a418312cad464f010d4.gif\" align=\"absmiddle\"/> 中性洗衣粉洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/2d00d5a4469d4a418312cad464f010d4.gif\" align=\"absmiddle\"/> 反面干</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/2d00d5a4469d4a418312cad464f010d4.gif\" align=\"absmiddle\"/> 干洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/2d00d5a4469d4a418312cad464f010d4.gif\" align=\"absmiddle\"/> 湿巾擦</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr></tbody></table><p><br/></p><p><strong><span style=\"color:#FF0000\">Y</span></strong> \n &nbsp;</p><p><br/></p><p><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/b591ecb5369d40c08008a43ec7fdea00.png\" height=\"27\" width=\"750\"/> \n </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/5bcbbf0c65464c24b70c975644c46370.jpg\"/> \n &nbsp;<br/> \n &nbsp;<img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/b8dbe6c55c2348f8bf1cf66e335d91f7.jpg\"/> \n &nbsp;<br/> &nbsp;\n &nbsp;<br/> &nbsp;\n &nbsp;<br/> \n &nbsp;<img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/b3e530f15b844a94a652b5bcb6353d60.jpg\"/> \n &nbsp;<br/> \n </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/bf0a14290a9f42e888cd884403b92e5e.png\" height=\"20\" width=\"750\"/> \n &nbsp;</p><center><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/1828889b9a7642a489de1d7583d2a54f.jpg\"/> \n &nbsp; <img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/5c2e9a6c22d94c98b50ded82de805c93.jpg\"/> \n &nbsp;</center><p><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/799903b7753c4d16943546fc62f9cd4e.jpg\"/> \n </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/4402fbcfdcf845cfbf1150b129287e13.png\" height=\"27\" width=\"750\"/> \n </p><p><span style=\"color:#000000\"><strong><a span=\"\" style=\"margin-left:5px;\">商品详细尺码</a></strong>(本商品的实际尺码) :</span> \n &nbsp;</p><table height=\"100%\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"85%\"><tbody><tr class=\"firstRow\"><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><p><span style=\"color:#000000\"><strong>商品详细尺码</strong>(本商品的实际尺码) :</span> </p>尺码实测位子SMLXL肩宽（Cm)----领型宽（Cm)----领型深（Cm)----胸围(胸单面x2)（Cm)82869296 腰围(腰单面x2)（Cm)74788486衣长（Cm)92939496</td></tr></tbody></table><p><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/0213f6970aab4687a22b42f2ff16e881.png\" height=\"27\" width=\"750\"/> \n </p><center><table cellspacing=\"0\" cellpadding=\"0\" width=\"600\"><tbody><tr class=\"firstRow\"><td colspan=\"3\"><br/></td></tr><tr><td colspan=\"3\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/894eefd277094d9f8b44f249c32038d0.png\"/></td></tr></tbody></table></center><p><br/></p>',0,0,NULL,NULL,NULL,'<p><strong><span style=\"color:#000000\">商品说明 :</span></strong><br/> </p><p>&nbsp;</p><p>短袖连衣裙，及膝款式，纽扣门襟，露肩吊带款式，肩带可调节，侧边隐形拉链开合，质地轻盈。</p><p>细节：</p><p>- 面料：100%粘性纤维；衬里：100%聚酯纤维</p><p>- 冷水机洗</p><p>- 模特信息：身高176cm，身穿尺码S</p><p>Forever21 Contemporary的尺码偏大，比Forever21的标准尺码要大一码，建议您选购Forever21 Contemporary的服装时小一号尺码。</p><p>- BY FOREVER21</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p><br/></p><table cellspacing=\"0\" cellpadding=\"0\" width=\"670\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 透明度 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/3a8964de046c4451bfc1d54b3792bd7a.gif\" align=\"absmiddle\"/> 有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/3a8964de046c4451bfc1d54b3792bd7a.gif\" align=\"absmiddle\"/> 稍微有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/261c6ba659a24746a051c37297205ad4.gif\" align=\"absmiddle\"/> 没有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 厚度感 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/3a8964de046c4451bfc1d54b3792bd7a.gif\" align=\"absmiddle\"/> &nbsp;厚</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/3a8964de046c4451bfc1d54b3792bd7a.gif\" align=\"absmiddle\"/> 中等</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/261c6ba659a24746a051c37297205ad4.gif\" align=\"absmiddle\"/> 薄</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 内衬 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/261c6ba659a24746a051c37297205ad4.gif\" align=\"absmiddle\"/> 有内衬</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/3a8964de046c4451bfc1d54b3792bd7a.gif\" align=\"absmiddle\"/> 没有内衬</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 弹力性 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/3a8964de046c4451bfc1d54b3792bd7a.gif\" align=\"absmiddle\"/> 有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/3a8964de046c4451bfc1d54b3792bd7a.gif\" align=\"absmiddle\"/> 稍微有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/261c6ba659a24746a051c37297205ad4.gif\" align=\"absmiddle\"/> 没有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 洗涤方法 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/3a8964de046c4451bfc1d54b3792bd7a.gif\" align=\"absmiddle\"/> 手洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/261c6ba659a24746a051c37297205ad4.gif\" align=\"absmiddle\"/> 用洗衣机洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/261c6ba659a24746a051c37297205ad4.gif\" align=\"absmiddle\"/> 凉水洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/3a8964de046c4451bfc1d54b3792bd7a.gif\" align=\"absmiddle\"/> 中性洗衣粉洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/3a8964de046c4451bfc1d54b3792bd7a.gif\" align=\"absmiddle\"/> 反面干</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/3a8964de046c4451bfc1d54b3792bd7a.gif\" align=\"absmiddle\"/> 干洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/3a8964de046c4451bfc1d54b3792bd7a.gif\" align=\"absmiddle\"/> 湿巾擦</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr></tbody></table><p><br/></p><p><strong><span style=\"color:#FF0000\">Y</span></strong> \n &nbsp;</p><p><br/></p><p><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/e71d900b7ade4c3e8e36ee2c556924b2.png\" height=\"27\" width=\"750\"/> \n </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/c4e5cbcfbda5439e9427423adce90b91.jpg\"/> \n &nbsp;<br/> \n &nbsp;<img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/cb54ad81b89d49ff9a8dc5c8b915eda7.jpg\"/> \n &nbsp;<br/> &nbsp;\n &nbsp;<br/> &nbsp;\n &nbsp;<br/> \n &nbsp;<img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/04586cf749904c0189e391e0d9e6a6b3.jpg\"/> \n &nbsp;<br/> \n </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/a920e33206ed4c47884e0d03b3ce6daf.png\" height=\"20\" width=\"750\"/> \n &nbsp;</p><center><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/7e4ebfff561b4a3a9e043d60a7d23b29.jpg\"/> \n &nbsp; <img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/82a05c86e46742a789c97e1c52e75e54.jpg\"/> \n &nbsp;</center><p><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/3f8cdf11bce94723824a48073dad5297.jpg\"/> \n </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/577750f3d4f24ef4b44f4f8cda9fd47a.png\" height=\"27\" width=\"750\"/> \n </p><p><span style=\"color:#000000\"><strong><a span=\"\" style=\"margin-left:5px;\">商品详细尺码</a></strong>(本商品的实际尺码) :</span> \n &nbsp;</p><table height=\"100%\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"85%\"><tbody><tr class=\"firstRow\"><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><p><span style=\"color:#000000\"><strong>商品详细尺码</strong>(本商品的实际尺码) :</span> </p>尺码实测位子SMLXL肩宽（Cm)----领型宽（Cm)----领型深（Cm)----胸围(胸单面x2)（Cm)82869296 腰围(腰单面x2)（Cm)74788486衣长（Cm)92939496</td></tr></tbody></table><p><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/4d986b315b60439ca0c763018067cd04.png\" height=\"27\" width=\"750\"/> \n </p><center><table cellspacing=\"0\" cellpadding=\"0\" width=\"600\"><tbody><tr class=\"firstRow\"><td colspan=\"3\"><br/></td></tr><tr><td colspan=\"3\"><img class=\"\" src=\"/Storage/Shop/1/Products/712/remark/7300fcd7a5714aff9b3e3b699b8366e1.png\"/></td></tr></tbody></table></center><p><br/></p>'),(0,713,'发布的商品不符合要求','<p><strong><span style=\"color:#000000\">商品说明 :</span></strong><br/> </p><p>&nbsp;</p><p>圆领无袖针织衫，撞色条纹图案，短款版型，罗纹材质，百搭款！</p><p>细节：</p><p>- 面料：71%粘性纤维，29%尼龙</p><p>- 冷水手洗</p><p>-BY FOREVER21</p><p>&nbsp;</p><p>&nbsp;</p><p><br/></p><table cellspacing=\"0\" cellpadding=\"0\" width=\"670\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 透明度 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/bea39bd64775450594b01d2306e7e89c.gif\" align=\"absmiddle\"/> 有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/bea39bd64775450594b01d2306e7e89c.gif\" align=\"absmiddle\"/> 稍微有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/9a2d64fc7c454c4fb81c774f7b536be0.gif\" align=\"absmiddle\"/> 没有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 厚度感 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/bea39bd64775450594b01d2306e7e89c.gif\" align=\"absmiddle\"/> &nbsp;厚</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/9a2d64fc7c454c4fb81c774f7b536be0.gif\" align=\"absmiddle\"/> 中等</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/bea39bd64775450594b01d2306e7e89c.gif\" align=\"absmiddle\"/> 薄</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 内衬 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/bea39bd64775450594b01d2306e7e89c.gif\" align=\"absmiddle\"/> 有内衬</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/9a2d64fc7c454c4fb81c774f7b536be0.gif\" align=\"absmiddle\"/> 没有内衬</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 弹力性 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/bea39bd64775450594b01d2306e7e89c.gif\" align=\"absmiddle\"/> 有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/9a2d64fc7c454c4fb81c774f7b536be0.gif\" align=\"absmiddle\"/> 稍微有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/bea39bd64775450594b01d2306e7e89c.gif\" align=\"absmiddle\"/> 没有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 洗涤方法 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/9a2d64fc7c454c4fb81c774f7b536be0.gif\" align=\"absmiddle\"/> 手洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/bea39bd64775450594b01d2306e7e89c.gif\" align=\"absmiddle\"/> 用洗衣机洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/9a2d64fc7c454c4fb81c774f7b536be0.gif\" align=\"absmiddle\"/> 凉水洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/bea39bd64775450594b01d2306e7e89c.gif\" align=\"absmiddle\"/> 中性洗衣粉洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/bea39bd64775450594b01d2306e7e89c.gif\" align=\"absmiddle\"/> 反面干</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/bea39bd64775450594b01d2306e7e89c.gif\" align=\"absmiddle\"/> 干洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/bea39bd64775450594b01d2306e7e89c.gif\" align=\"absmiddle\"/> 湿巾擦</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr></tbody></table><p><br/></p><p><strong><span style=\"color:#FF0000\">Y</span></strong> \n &nbsp;</p><p><br/></p><p><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/f8a1003878114417bac6f1f0085873c3.gif\" height=\"27\" width=\"750\"/> \n </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/a6a4811089ef4035a3a319b75d17cf9a.jpg\"/> \n &nbsp;<br/> \n &nbsp;<img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/60183984dbf64221ad3f6198a2119171.jpg\"/> \n &nbsp;<br/> &nbsp;\n &nbsp;<br/> \n &nbsp;<img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/319a485b98214ea3a7709ab7d4182d8d.jpg\"/> \n &nbsp;<br/> \n &nbsp;<img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/f2a63d13bb6349329a968db3c1fc1bb0.jpg\"/> \n &nbsp;<br/> \n </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/5e15d1c578b749af86aca8010de64499.gif\" height=\"20\" width=\"750\"/> \n &nbsp;</p><center><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/08b16345b05447d397324bc158a22c34.jpg\"/> \n &nbsp; <img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/e138fb830b5e413f98ab6925828106ea.jpg\"/> \n &nbsp; <br/> \n &nbsp; <img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/0dd9d9f27cb14493b7e638628c5e1531.jpg\"/> \n &nbsp;</center><p><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/9419c639689f4a758780c7d4229bd5e5.jpg\"/> \n </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/6c21b4ad6d7c449fa11d73a13d875a5d.gif\" height=\"27\" width=\"750\"/> \n </p><p><span style=\"color:#000000\"><strong><a span=\"\" style=\"margin-left:5px;\">商品详细尺码</a></strong>(本商品的实际尺码) :</span> \n &nbsp;</p><table height=\"100%\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"85%\"><tbody><tr class=\"firstRow\"><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><p><span style=\"color:#000000\"><strong>商品详细尺码</strong>(本商品的实际尺码) :</span> </p>尺码实测位子SML肩宽（Cm)232526领型宽（Cm)121314领型深（Cm)777胸围(胸单面x2)（Cm)525662衣长（Cm)394244</td></tr></tbody></table><p><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/a0685e9b91a046b0b25188923e34e567.gif\" height=\"27\" width=\"750\"/> \n </p><center><table cellspacing=\"0\" cellpadding=\"0\" width=\"600\"><tbody><tr class=\"firstRow\"><td colspan=\"3\"><br/></td></tr><tr><td colspan=\"3\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/e6687144025a4d59a459ae893931d405.gif\"/></td></tr></tbody></table></center>',0,0,NULL,NULL,NULL,'<p><strong><span style=\"color:#000000\">商品说明 :</span></strong><br/> </p><p>&nbsp;</p><p>圆领无袖针织衫，撞色条纹图案，短款版型，罗纹材质，百搭款！</p><p>细节：</p><p>- 面料：71%粘性纤维，29%尼龙</p><p>- 冷水手洗</p><p>-BY FOREVER21</p><p>&nbsp;</p><p>&nbsp;</p><p><br/></p><table cellspacing=\"0\" cellpadding=\"0\" width=\"670\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 透明度 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/aa084539ae36474e8e38b87ead835288.gif\" align=\"absmiddle\"/> 有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/aa084539ae36474e8e38b87ead835288.gif\" align=\"absmiddle\"/> 稍微有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/22d679f251004f76b9b44a45f2d67d40.gif\" align=\"absmiddle\"/> 没有透明度</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 厚度感 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/aa084539ae36474e8e38b87ead835288.gif\" align=\"absmiddle\"/> &nbsp;厚</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/22d679f251004f76b9b44a45f2d67d40.gif\" align=\"absmiddle\"/> 中等</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/aa084539ae36474e8e38b87ead835288.gif\" align=\"absmiddle\"/> 薄</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 内衬 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/aa084539ae36474e8e38b87ead835288.gif\" align=\"absmiddle\"/> 有内衬</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/22d679f251004f76b9b44a45f2d67d40.gif\" align=\"absmiddle\"/> 没有内衬</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 弹力性 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/aa084539ae36474e8e38b87ead835288.gif\" align=\"absmiddle\"/> 有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/22d679f251004f76b9b44a45f2d67d40.gif\" align=\"absmiddle\"/> 稍微有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/aa084539ae36474e8e38b87ead835288.gif\" align=\"absmiddle\"/> 没有弹力性</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr><tr><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\">- 洗涤方法 :</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/22d679f251004f76b9b44a45f2d67d40.gif\" align=\"absmiddle\"/> 手洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/aa084539ae36474e8e38b87ead835288.gif\" align=\"absmiddle\"/> 用洗衣机洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/22d679f251004f76b9b44a45f2d67d40.gif\" align=\"absmiddle\"/> 凉水洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/aa084539ae36474e8e38b87ead835288.gif\" align=\"absmiddle\"/> 中性洗衣粉洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/aa084539ae36474e8e38b87ead835288.gif\" align=\"absmiddle\"/> 反面干</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/aa084539ae36474e8e38b87ead835288.gif\" align=\"absmiddle\"/> 干洗</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/aa084539ae36474e8e38b87ead835288.gif\" align=\"absmiddle\"/> 湿巾擦</td><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><br/></td></tr></tbody></table><p><br/></p><p><strong><span style=\"color:#FF0000\">Y</span></strong> \n &nbsp;</p><p><br/></p><p><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/4ec87076cfeb48e8bb85cb2ff9510098.gif\" height=\"27\" width=\"750\"/> \n </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/52f3ee0c3f4842e3b00e39cd6ba9c21f.jpg\"/> \n &nbsp;<br/> \n &nbsp;<img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/28bd1cb2f4804097be855241534d5062.jpg\"/> \n &nbsp;<br/> &nbsp;\n &nbsp;<br/> \n &nbsp;<img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/c80119efd7dc4f5183bde04b965a2b26.jpg\"/> \n &nbsp;<br/> \n &nbsp;<img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/3f4f2b59da8b4979a48da4cfc40e060c.jpg\"/> \n &nbsp;<br/> \n </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/11ef7ff81cc14439aa2e77494ffbff83.gif\" height=\"20\" width=\"750\"/> \n &nbsp;</p><center><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/1a756e8a49884fb8b1a277bfb67f14ab.jpg\"/> \n &nbsp; <img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/1e6b201f8e884f3db864c9956cdf9ca0.jpg\"/> \n &nbsp; <br/> \n &nbsp; <img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/d0c5843afa1a4a8abdf7ea303640527c.jpg\"/> \n &nbsp;</center><p><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/b5e00c3d9b754b129cfed1500e999eeb.jpg\"/> \n </p><p><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/3aa61cae9f6d4e9482e5c557d0dd7932.gif\" height=\"27\" width=\"750\"/> \n </p><p><span style=\"color:#000000\"><strong><a span=\"\" style=\"margin-left:5px;\">商品详细尺码</a></strong>(本商品的实际尺码) :</span> \n &nbsp;</p><table height=\"100%\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" width=\"85%\"><tbody><tr class=\"firstRow\"><td style=\"font-family: &quot;Microsoft YaHei&quot;, simsun, Verdana; font-size: 11px; color: rgb(87, 87, 87);\"><p><span style=\"color:#000000\"><strong>商品详细尺码</strong>(本商品的实际尺码) :</span> </p>尺码实测位子SML肩宽（Cm)232526领型宽（Cm)121314领型深（Cm)777胸围(胸单面x2)（Cm)525662衣长（Cm)394244</td></tr></tbody></table><p><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/d1f6ee822e4640e28e167ee81e88e731.gif\" height=\"27\" width=\"750\"/> \n </p><center><table cellspacing=\"0\" cellpadding=\"0\" width=\"600\"><tbody><tr class=\"firstRow\"><td colspan=\"3\"><br/></td></tr><tr><td colspan=\"3\"><img class=\"\" src=\"/Storage/Shop/1/Products/713/remark/f6a5c45cc20442e0adfe348824ec3894.gif\"/></td></tr></tbody></table></center>'),(0,714,'发布的商品不符合要求','<ul class=\"parameter2 p-parameter-list list-paddingleft-2\"><li><p>商品名称：AppleMacBook Air</p></li><li><p>商品编号：2342601</p></li><li><p>商品毛重：3.0kg</p></li><li><p>商品产地：中国大陆</p></li><li><p>系统：其他</p></li><li><p>厚度：15.1mm—20.0mm</p></li><li><p>内存容量：8G</p></li><li><p>分辨率：其他</p></li><li><p>显卡型号：其他</p></li><li><p>待机时长：9小时以上</p></li><li><p>处理器：Intel i5低功耗版</p></li><li><p>特性：背光键盘</p></li><li><p>显卡类别：集成显卡</p></li><li><p>裸机重量：1-1.5KG</p></li><li><p>硬盘容量：128G固态</p></li><li><p>显存容量：其他</p></li><li><p>分类：轻薄本</p></li><li><p>屏幕尺寸：13.3英寸</p></li></ul><p class=\"more-par\"><a href=\"https://item.jd.com/2342601.html#product-detail\" class=\"J-more-param\">更多参数<span style=\"text-decoration:line-through;\">&gt;&gt;</span></a>\n &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p><p><a href=\"https://sale.jd.com/act/kxrtIQs6FcH.html\" target=\"_blank\"><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/714/remark/17d9bd614ffa4aa2b68ffe570033bbd6.jpg\"/></a><br/></p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/714/remark/b54a96f5fac84906be5cb727d11edd64.jpg\"/><br/></p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/714/remark/76a30672f7924f90865fdcb32ab87a0b.jpg\"/> &nbsp;</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/714/remark/26b0d1c7f81a4d4f953d92df4cdbce5f.jpg\"/> &nbsp;</p>',0,0,NULL,NULL,NULL,'<ul class=\"parameter2 p-parameter-list list-paddingleft-2\"><li><p>商品名称：AppleMacBook Air</p></li><li><p>商品编号：2342601</p></li><li><p>商品毛重：3.0kg</p></li><li><p>商品产地：中国大陆</p></li><li><p>系统：其他</p></li><li><p>厚度：15.1mm—20.0mm</p></li><li><p>内存容量：8G</p></li><li><p>分辨率：其他</p></li><li><p>显卡型号：其他</p></li><li><p>待机时长：9小时以上</p></li><li><p>处理器：Intel i5低功耗版</p></li><li><p>特性：背光键盘</p></li><li><p>显卡类别：集成显卡</p></li><li><p>裸机重量：1-1.5KG</p></li><li><p>硬盘容量：128G固态</p></li><li><p>显存容量：其他</p></li><li><p>分类：轻薄本</p></li><li><p>屏幕尺寸：13.3英寸</p></li></ul><p class=\"more-par\"><a href=\"https://item.jd.com/2342601.html#product-detail\" class=\"J-more-param\">更多参数<span style=\"text-decoration:line-through;\">&gt;&gt;</span></a>\n &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p><p><a href=\"https://sale.jd.com/act/kxrtIQs6FcH.html\" target=\"_blank\"><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/714/remark/bf84e272a9cb46069f3c5c30b2632608.jpg\"/></a><br/></p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/714/remark/5cdd1700e20b4b2390db3cca043b7161.jpg\"/><br/></p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/714/remark/83f40cde65734bae9fe5bb86f291012e.jpg\"/> &nbsp;</p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/714/remark/ed7255406f4b468fa0a19f45ae62a062.jpg\"/> &nbsp;</p>'),(0,715,NULL,'<ul class=\"parameter2 p-parameter-list list-paddingleft-2\"><li><p>商品名称：AppleMacBook Pro</p></li><li><p>商品编号：1593516</p></li><li><p>商品毛重：3.96kg</p></li><li><p>商品产地：中国大陆</p></li><li><p>系统：其他</p></li><li><p>厚度：15.1mm—20.0mm</p></li><li><p>内存容量：16G</p></li><li><p>分辨率：超高清屏（2K/3k/4K)</p></li><li><p>显卡型号：其他</p></li><li><p>待机时长：9小时以上</p></li><li><p>处理器：Intel i7标准电压版</p></li><li><p>特性：背光键盘</p></li><li><p>显卡类别：集成显卡</p></li><li><p>裸机重量：2-2.5kg</p></li><li><p>硬盘容量：256G固态</p></li><li><p>显存容量：其他</p></li><li><p>分类：轻薄本</p></li><li><p>屏幕尺寸：其他</p></li></ul><p class=\"more-par\"><a href=\"https://item.jd.com/1593516.html#product-detail\" class=\"J-more-param\">更多参数<span style=\"text-decoration:line-through;\">&gt;&gt;</span></a>\n &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p><p><a href=\"https://sale.jd.com/act/kxrtIQs6FcH.html\" target=\"_blank\"><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/715/remark/8f3cc27726434e2f86693f3c88544b3d.jpg\"/></a><br/></p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/715/remark/3844ef19b03d4988a7c7733ea8270690.jpg\"/><br/></p><p><img class=\"\" src=\"/Storage/Shop/1/Products/715/remark/b9dd0e783bae4900b23b6626ecb6d49c.jpg\"/></p><p><img class=\"\" src=\"/Storage/Shop/1/Products/715/remark/b67006f597854d0c86903d5515d3098d.jpg\"/></p>',0,0,NULL,NULL,NULL,'<ul class=\"parameter2 p-parameter-list list-paddingleft-2\"><li><p>商品名称：AppleMacBook Pro</p></li><li><p>商品编号：1593516</p></li><li><p>商品毛重：3.96kg</p></li><li><p>商品产地：中国大陆</p></li><li><p>系统：其他</p></li><li><p>厚度：15.1mm—20.0mm</p></li><li><p>内存容量：16G</p></li><li><p>分辨率：超高清屏（2K/3k/4K)</p></li><li><p>显卡型号：其他</p></li><li><p>待机时长：9小时以上</p></li><li><p>处理器：Intel i7标准电压版</p></li><li><p>特性：背光键盘</p></li><li><p>显卡类别：集成显卡</p></li><li><p>裸机重量：2-2.5kg</p></li><li><p>硬盘容量：256G固态</p></li><li><p>显存容量：其他</p></li><li><p>分类：轻薄本</p></li><li><p>屏幕尺寸：其他</p></li></ul><p class=\"more-par\"><a href=\"https://item.jd.com/1593516.html#product-detail\" class=\"J-more-param\">更多参数<span style=\"text-decoration:line-through;\">&gt;&gt;</span></a>\n &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p><p><a href=\"https://sale.jd.com/act/kxrtIQs6FcH.html\" target=\"_blank\"><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/715/remark/8e84e51ca67540998c2cc498a94983d6.jpg\"/></a><br/></p><p><img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/715/remark/d93987f539fe4b60b07a407ef8372bf0.jpg\"/><br/></p><p><img class=\"\" src=\"/Storage/Shop/1/Products/715/remark/db003c8ff80c4e05b2d17fc48003b59c.jpg\"/></p><p><img class=\"\" src=\"/Storage/Shop/1/Products/715/remark/4900223c56de450981130ce9e499716a.jpg\"/></p>'),(0,716,NULL,'<p><br/> <img alt=\"\" id=\"eb6cda93158c47caa5761c189cd30e8c\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/c25927423c8d4f519047d9935c155efb.jpg\"/> <br/> <img alt=\"\" id=\"243444ceb6864c57af82d10b55d6ca82\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/2f67c0e04c7a4baa99e6ea23570dfcbb.png\"/>&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp; \n<img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/75458d07b5b94c05bb914160122909c5.jpg\"/> <img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/9ffa54a97a80477f87d5bbd3a14a7aa5.jpg\"/> <img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/97fb049db3924197ada91c357f5ec920.jpg\"/> <img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/b99cafdffc4b4909aaa81fda7cd4ff91.jpg\"/> <img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/ce864c1f44e74821ba57292aa6a6f5dd.jpg\"/> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; \n<img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/3c99763f3025485d9ea0d48f2df88db9.jpg\"/> <img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/03221bde218d43d5aaeb9bd6a3b7c52f.jpg\"/> <img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/e2c203b5ac964e3daf66bf5cae6c22d6.jpg\"/> <br/> <img alt=\"\" id=\"0bf2b038288b451e9a5d42fc4c4def10\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/353dab547b9b44ef98a643940f5d84c3.jpg\"/> <br/> <img alt=\"\" id=\"79223096476d4a15b61f8f9937a1b693\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/2e07562fc19240348f51c958c26d252d.jpg\"/> <br/> &nbsp;&nbsp; \n<br/> &nbsp;&nbsp; \n<br/> <img alt=\"\" id=\"c741c5bab80c4e64b291c0b7d95e4a5e\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/93c1d261652c433781d4511b5fb2027a.jpg\"/></p>',0,0,NULL,NULL,NULL,'<p><br/> <img alt=\"\" id=\"eb6cda93158c47caa5761c189cd30e8c\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/5b320c3049194d2f83e477688124f816.jpg\"/> <br/> <img alt=\"\" id=\"243444ceb6864c57af82d10b55d6ca82\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/f132a2c1f8974003b53357d93074d1fc.png\"/>&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp; \n<img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/64bfa716c009430c8d14d1c4a55e05a1.jpg\"/> <img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/c0a43006495b4abc85d2681814b7af3c.jpg\"/> <img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/08ef5f69966f46d182d1f1332dfbd905.jpg\"/> <img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/e88c8da2a09249959a618e84eadf16c1.jpg\"/> <img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/4d3cf3db4aba4851955cf0e7ac111ece.jpg\"/> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; \n<img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/f7b2ccf4e583405fa251dc017afae371.jpg\"/> <img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/54dec48c41fc4cac8760118acf06c1f0.jpg\"/> <img alt=\"\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/63af58da22124120889514211c0c7a14.jpg\"/> <br/> <img alt=\"\" id=\"0bf2b038288b451e9a5d42fc4c4def10\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/d53e43f20c3a46158bc28f35df74316c.jpg\"/> <br/> <img alt=\"\" id=\"79223096476d4a15b61f8f9937a1b693\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/fd861621ad4145fe857bc8095d760134.jpg\"/> <br/> &nbsp;&nbsp; \n<br/> &nbsp;&nbsp; \n<br/> <img alt=\"\" id=\"c741c5bab80c4e64b291c0b7d95e4a5e\" class=\"\" src=\"/Storage/Shop/1/Products/716/remark/9e0934dc0fc44ff09149dd78e54e6600.jpg\"/></p>'),(0,717,'发布的商品不符合要求','<ul id=\"parameter2\" class=\"p-parameter-list list-paddingleft-2\"><li><p>商品名称：欧莱雅水能润泽双效洁面膏100+50ml</p></li><li><p>商品编号：1019914947</p></li><li><p>商品毛重：100.00g</p></li><li><p>适合肤质：任何肤质</p></li><li><p>性别：男</p></li></ul><p class=\"more-par\"><a href=\"http://item.jd.com/1019914947.html?jd_pop=61d106a6-3638-4aca-ac44-2b362bd84ad4&abt=0#product-detail\" class=\"J-more-param\">更多参数&gt;&gt;</a></p><p><br/></p><p><br/><img data-lazyload=\"done\" alt=\"\" id=\"7f8d04f4971649d2947ec360ef847562\n\" src=\"/Storage/Shop/1/Products/717/remark/5319de9cbe7c4d8290726f8566a46364.jpg\"/></p>',0,0,NULL,NULL,NULL,'<ul id=\"parameter2\" class=\"p-parameter-list list-paddingleft-2\" style=\"width: 608.938px; white-space: normal;\"><li><p>商品名称：欧莱雅水能润泽双效洁面膏100+50ml</p></li><li><p>商品编号：1019914947</p></li><li><p>商品毛重：100.00g</p></li><li><p>适合肤质：任何肤质</p></li><li><p>性别：男</p></li></ul><p class=\"more-par\" style=\"white-space: normal;\"><a href=\"http://item.jd.com/1019914947.html?jd_pop=61d106a6-3638-4aca-ac44-2b362bd84ad4&abt=0#product-detail\" class=\"J-more-param\">更多参数&gt;&gt;</a></p><p style=\"white-space: normal;\"><br/></p><p style=\"white-space: normal;\"><br/><img data-lazyload=\"done\" alt=\"\" id=\"7f8d04f4971649d2947ec360ef847562\n\" src=\"/Storage/Shop/1/Products/717/remark/5f80437d52bc4fbbb5e63e591f6efb63.jpg\"/></p>'),(0,718,NULL,'<ul id=\"parameter2\" class=\"p-parameter-list list-paddingleft-2\"><li><p><br/></p></li><li><p>商品名称：贝德玛卸妆水</p></li><li><p>商品编号：1938255</p></li><li><p>品牌： <a href=\"https://list.jd.com/list.html?cat=1316,1381,13544&ev=exbrand_4583\" target=\"_blank\">贝德玛（BIODERMA）</a></p></li><li><p>商品毛重：500.00g</p></li><li><p>商品产地：法国</p></li><li><p>适合肤质：任何肤质</p></li><li><p>功效：深层清洁</p></li><li><p>产品产地：欧美</p></li><li><p>分类：洁面乳</p></li><li><p>质地：液/露状</p></li><li><p>性别：通用</p></li><li><p><br/></p></li></ul><p><br/></p><p><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/718/remark/cf9915c64dda4836a2ac16d563896ebe.jpg\"/><br/></p><p><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/718/remark/bf75714df4474510b392d12e3f815ded.jpg\"/></p>',0,0,NULL,NULL,NULL,'<ul id=\"parameter2\" class=\"p-parameter-list list-paddingleft-2\"><li><p><br/></p></li><li><p>商品名称：贝德玛卸妆水</p></li><li><p>商品编号：1938255</p></li><li><p>品牌： <a href=\"https://list.jd.com/list.html?cat=1316,1381,13544&ev=exbrand_4583\" target=\"_blank\">贝德玛（BIODERMA）</a></p></li><li><p>商品毛重：500.00g</p></li><li><p>商品产地：法国</p></li><li><p>适合肤质：任何肤质</p></li><li><p>功效：深层清洁</p></li><li><p>产品产地：欧美</p></li><li><p>分类：洁面乳</p></li><li><p>质地：液/露状</p></li><li><p>性别：通用</p></li><li><p><br/></p></li></ul><p><br/></p><p><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/718/remark/91630e8294ea4afd9e602763fab2e523.jpg\"/><br/></p><p><img data-lazyload=\"done\" alt=\"\" src=\"/Storage/Shop/1/Products/718/remark/bc01b119a70a413baba4af3c49750685.jpg\"/></p>'),(0,719,NULL,'<p><img alt=\"资生堂、洁面乳、护肤步骤、美容护肤\" src=\"/Storage/Shop/1/Products/719/remark/8fd8dffb6c894276a1d16401016d5f1e.jpg\" height=\"2039\" width=\"750\"/><br/><img alt=\"资生堂、洁面乳、护肤步骤、美容护肤\" src=\"/Storage/Shop/1/Products/719/remark/1c80cf97bc624439a283c87e2d8211a0.jpg\" height=\"1343\" width=\"750\"/><br/><img alt=\"资生堂、洁面乳、护肤步骤、美容护肤\" src=\"/Storage/Shop/1/Products/719/remark/a7843fe089db4980a2cafc3b7617b923.jpg\" height=\"1543\" width=\"750\"/><br/><br/></p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p><br/><img alt=\"\" src=\"/Storage/Shop/1/Products/719/remark/a5e2f362cd984693bebe12d2c1138b93.jpg\" height=\"98\" width=\"750\"/></p><p style=\";background: white\"><strong><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;color: #e4007f;font-size: 13px\">卸妆</span></strong></p><p style=\";background: white\"><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;font-size: 13px\">面部卸妆是最基本的清洁环节，对面部进行卸妆前，一定要认真阅读卸妆用品卸妆说明书，因为不同的产品使用的方法也不尽相同。<br/> 在鼻子两侧要用卸妆乳上下涂抹，其他部位由内向外轻揉，待污垢完全与卸妆乳融合再擦掉或冲洗掉。</span></p><p style=\";background: white\"><strong><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;color: #e4007f;font-size: 13px\">洁面</span></strong></p><p style=\";background: white\"><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;font-size: 13px\">通过清洁使得皮肤处于尽可能无污染和无侵害的状态中，为皮肤提供良好的生理条件，清洁皮肤也是对皮肤很好的调整和放松过程，<br/> 有效地激发皮肤活力，使得毛孔充分通透，充分发挥皮肤健康和正常的吸收、呼吸、排泄功能，保持皮肤良好的新陈代谢状态。</span></p><p style=\";background: white\"><strong><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;color: #e4007f;font-size: 13px\">爽肤水</span></strong></p><p style=\";background: white\"><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;font-size: 13px\">爽肤水它的作用就在于再次清洁以恢复肌肤表面的酸碱值，并调理角质层，使肌肤更好地吸收，并为使用保养品作准备。专家建议<br/> 油性皮肤使用紧肤水，健康皮肤使用爽肤水，干性皮肤使用柔肤水，混合皮肤<span style=\"color: windowtext\">T</span><span style=\"color: windowtext\">区</span>使用紧肤水，敏感皮肤选用敏感水、修复水。</span></p><p style=\";background: white\"><strong><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;color: #e4007f;font-size: 13px\">精华</span></strong></p><p class=\"MsoListParagraph\" style=\"text-indent: 0;background: white\"><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;font-size: 13px\">精华素应该在清洁完皮肤，均匀涂抹柔肤水、软肤水后使用，切忌洁肤后马上使用。能够帮助皮肤形成皮脂膜，从而有效地吸收水<br/> 分去除老旧角质，辅助肌肤吸收精华素的营养，令精华素的养分更充分、直接地进入皮肤深层，令皮肤的柔软性、弹性更好。</span></p><p style=\";background: white\"><strong><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;color: #e4007f;font-size: 13px\">乳液</span></strong></p><p class=\"MsoListParagraph\" style=\"text-indent: 0;background: white\"><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;font-size: 13px\">最大的特点就是含水量很高，可以瞬间滋润肌肤，为干燥肌肤补充水分。加上乳液可以在肌肤表面形成轻薄透气的保护膜，可防止<br/> 水分流失，起到保湿效果。</span></p><p class=\"MsoListParagraph\" style=\"text-indent: 0;background: white\"><strong><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;color: #e4007f;font-size: 13px\">面霜</span></strong></p><p class=\"MsoListParagraph\" style=\"text-indent: 0;background: white\"><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;font-size: 13px\">一般来说， 干性皮肤适合使用面霜以及较为滋润的乳液，油性及混合性皮肤适合清爽的乳液及不含油分的面霜。干燥季节，如秋冬<br/> 季节，适合使用质地较厚的面霜。</span></p><p class=\"MsoListParagraph\" style=\"text-indent: 0;background: white\"><strong><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;color: #e4007f;font-size: 13px\">防晒隔离</span></strong></p><p class=\"MsoListParagraph\" style=\"text-indent: 0;background: white\"><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;font-size: 13px\">隔离霜大致分两种，有一种又称妆前底乳（Make-upBase），因为早先的彩妆产品都含有会伤皮肤的成分，所以“发明”了在<span style=\"color: windowtext\">护肤</span><br/> 与彩妆品之间加一道隔离霜以隔离彩妆和脏空气，也能使妆容更加细腻、服贴。另一种其实就是防晒霜 ，因为含有或完全以物理防<br/> 晒剂来阻隔紫外线，而被称为防晒隔离霜（UVBlock或SunBlock）。</span></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p><img alt=\"资生堂、洁面乳、护肤步骤、美容护肤\" src=\"/Storage/Shop/1/Products/719/remark/a90dbdb3ce4b40448c59171f5bb21315.jpg\" height=\"2039\" width=\"750\"/><br/><img alt=\"资生堂、洁面乳、护肤步骤、美容护肤\" src=\"/Storage/Shop/1/Products/719/remark/19844539819646d4bb63d3f81bd2a180.jpg\" height=\"1343\" width=\"750\"/><br/><img alt=\"资生堂、洁面乳、护肤步骤、美容护肤\" src=\"/Storage/Shop/1/Products/719/remark/11b01b177fac47e497a988cd40abe91e.jpg\" height=\"1543\" width=\"750\"/><br/><br/></p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p><br/><img alt=\"\" src=\"/Storage/Shop/1/Products/719/remark/34c2a1488f5d4e288fbbb7cfc9b7135c.jpg\" height=\"98\" width=\"750\"/></p><p style=\";background: white\"><strong><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;color: #e4007f;font-size: 13px\">卸妆</span></strong></p><p style=\";background: white\"><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;font-size: 13px\">面部卸妆是最基本的清洁环节，对面部进行卸妆前，一定要认真阅读卸妆用品卸妆说明书，因为不同的产品使用的方法也不尽相同。<br/> 在鼻子两侧要用卸妆乳上下涂抹，其他部位由内向外轻揉，待污垢完全与卸妆乳融合再擦掉或冲洗掉。</span></p><p style=\";background: white\"><strong><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;color: #e4007f;font-size: 13px\">洁面</span></strong></p><p style=\";background: white\"><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;font-size: 13px\">通过清洁使得皮肤处于尽可能无污染和无侵害的状态中，为皮肤提供良好的生理条件，清洁皮肤也是对皮肤很好的调整和放松过程，<br/> 有效地激发皮肤活力，使得毛孔充分通透，充分发挥皮肤健康和正常的吸收、呼吸、排泄功能，保持皮肤良好的新陈代谢状态。</span></p><p style=\";background: white\"><strong><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;color: #e4007f;font-size: 13px\">爽肤水</span></strong></p><p style=\";background: white\"><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;font-size: 13px\">爽肤水它的作用就在于再次清洁以恢复肌肤表面的酸碱值，并调理角质层，使肌肤更好地吸收，并为使用保养品作准备。专家建议<br/> 油性皮肤使用紧肤水，健康皮肤使用爽肤水，干性皮肤使用柔肤水，混合皮肤<span style=\"color: windowtext\">T</span><span style=\"color: windowtext\">区</span>使用紧肤水，敏感皮肤选用敏感水、修复水。</span></p><p style=\";background: white\"><strong><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;color: #e4007f;font-size: 13px\">精华</span></strong></p><p class=\"MsoListParagraph\" style=\"text-indent: 0;background: white\"><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;font-size: 13px\">精华素应该在清洁完皮肤，均匀涂抹柔肤水、软肤水后使用，切忌洁肤后马上使用。能够帮助皮肤形成皮脂膜，从而有效地吸收水<br/> 分去除老旧角质，辅助肌肤吸收精华素的营养，令精华素的养分更充分、直接地进入皮肤深层，令皮肤的柔软性、弹性更好。</span></p><p style=\";background: white\"><strong><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;color: #e4007f;font-size: 13px\">乳液</span></strong></p><p class=\"MsoListParagraph\" style=\"text-indent: 0;background: white\"><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;font-size: 13px\">最大的特点就是含水量很高，可以瞬间滋润肌肤，为干燥肌肤补充水分。加上乳液可以在肌肤表面形成轻薄透气的保护膜，可防止<br/> 水分流失，起到保湿效果。</span></p><p class=\"MsoListParagraph\" style=\"text-indent: 0;background: white\"><strong><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;color: #e4007f;font-size: 13px\">面霜</span></strong></p><p class=\"MsoListParagraph\" style=\"text-indent: 0;background: white\"><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;font-size: 13px\">一般来说， 干性皮肤适合使用面霜以及较为滋润的乳液，油性及混合性皮肤适合清爽的乳液及不含油分的面霜。干燥季节，如秋冬<br/> 季节，适合使用质地较厚的面霜。</span></p><p class=\"MsoListParagraph\" style=\"text-indent: 0;background: white\"><strong><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;color: #e4007f;font-size: 13px\">防晒隔离</span></strong></p><p class=\"MsoListParagraph\" style=\"text-indent: 0;background: white\"><span style=\"font-family: &#39;微软雅黑&#39;,&#39;sans-serif&#39;;font-size: 13px\">隔离霜大致分两种，有一种又称妆前底乳（Make-upBase），因为早先的彩妆产品都含有会伤皮肤的成分，所以“发明”了在<span style=\"color: windowtext\">护肤</span><br/> 与彩妆品之间加一道隔离霜以隔离彩妆和脏空气，也能使妆容更加细腻、服贴。另一种其实就是防晒霜 ，因为含有或完全以物理防<br/> 晒剂来阻隔紫外线，而被称为防晒隔离霜（UVBlock或SunBlock）。</span></p><p><br/></p>'),(0,720,NULL,'<ul class=\"cnt clearfix list-paddingleft-2\"><li><p></p></li><li><p>品牌：<a href=\"http://www.suning.com/pinpai/7128-0-0.html\" target=\"_blank\">欧莱雅(L&#39;OREAL)</a></p></li><li><p></p></li><li><p>类别：洁面膏</p></li><li><p></p></li><li><p>适用人群：男士</p></li><li><p></p></li><li><p>规格：100ml</p></li><li><p></p></li><li><p>功效：保湿补水,滋润营养,缓解痘痘,控油平衡,深层清洁</p></li><li><p></p></li></ul><p>	</p><p>\n		\n	\n	\n	</p><p>\n			</p><p><br/> <br/><img alt=\"\" src=\"/Storage/Shop/1/Products/720/remark/92f6fba280e64269963da8862194698e.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/720/remark/5b10b61510a64dbcb9914edc619b3800.jpg\" class=\"err-product\" height=\"547\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/720/remark/7469d19b0da242e1986a2dd5caec1ef7.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/720/remark/0d22be7d1f5e41c29e19e159db07ec75.jpg\" class=\"err-product\" height=\"530\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/720/remark/4ca48a1195b84ace9db48ed9caf6da6f.jpg\" class=\"err-product\" height=\"998\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/720/remark/7f625d8375f440a6b9e3f14be6c1cde4.jpg\" class=\"err-product\" height=\"859\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/720/remark/7a08f8b85fb847aeb411852f90b9e916.jpg\" class=\"err-product\" height=\"1146\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/720/remark/be7339e3e1204fbfa7874dea04f5d5e6.jpg\" class=\"err-product\" height=\"298\" width=\"750\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<ul class=\"cnt clearfix list-paddingleft-2\"><li><p></p></li><li><p>品牌：<a href=\"http://www.suning.com/pinpai/7128-0-0.html\" target=\"_blank\">欧莱雅(L&#39;OREAL)</a></p></li><li><p></p></li><li><p>类别：洁面膏</p></li><li><p></p></li><li><p>适用人群：男士</p></li><li><p></p></li><li><p>规格：100ml</p></li><li><p></p></li><li><p>功效：保湿补水,滋润营养,缓解痘痘,控油平衡,深层清洁</p></li><li><p></p></li></ul><p>	</p><p>\n		\n	\n	\n	</p><p>\n			</p><p><br/> <br/><img alt=\"\" src=\"/Storage/Shop/1/Products/720/remark/09ecc6b562104d61be138ab086bbc080.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/720/remark/a2eec1d949514d5bb104d2a6a6c7fe95.jpg\" class=\"err-product\" height=\"547\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/720/remark/43f5523116be47f4bcd35677a0a105a0.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/720/remark/c6895b3a9af74c71928bd96f231df3e2.jpg\" class=\"err-product\" height=\"530\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/720/remark/e6ae8a1cad064cd89fc89c2894bf3374.jpg\" class=\"err-product\" height=\"998\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/720/remark/2019344aa6ae498688dbbf393699aea9.jpg\" class=\"err-product\" height=\"859\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/720/remark/18060f9599324a47a00decb1458ba6e3.jpg\" class=\"err-product\" height=\"1146\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/720/remark/55c56b6f4fa44d429d12357b53688174.jpg\" class=\"err-product\" height=\"298\" width=\"750\"/></p><p><br/></p>'),(0,721,NULL,'<p>	</p><p>\n		\n	\n	\n	</p><p>\n			</p><p><span style=\"font-size: 14px;\"><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/4f55af65d96c4581af80568718eb6cec.jpg\" class=\"err-product\" height=\"554\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/d2665882505246d68c04f917e99a3f3e.jpg\" class=\"err-product\" height=\"503\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/ca83b1e5dbcf438cae4f8fb0bef2fd3b.jpg\" class=\"err-product\" height=\"402\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/eee3873e6ba146abb367944e014f239f.jpg\" class=\"err-product\" height=\"490\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/4e180a2a8e7048e8809beaa406fa6a38.jpg\" class=\"err-product\" height=\"689\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/f0c092c5e4f74f82917c005ab5969285.jpg\" class=\"err-product\" height=\"609\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/cbd3b55ad4fa4790973ccde24b5b8a96.jpg\" class=\"err-product\" height=\"636\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/d85b46909c244f61b425acf4fdc9eb71.jpg\" class=\"err-product\" height=\"542\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/2b6605b8ae814b35917405cc687a30b9.jpg\" class=\"err-product\" height=\"618\" width=\"750\"/></span></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p>	</p><p>\n		\n	\n	\n	</p><p>\n			</p><p><span style=\"font-size: 14px;\"><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/8305792eb6bd44438189c65ac039a83a.jpg\" class=\"err-product\" height=\"554\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/4aa61446f853480eb4ac8d90031b5a92.jpg\" class=\"err-product\" height=\"503\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/b23f9be772dc4af089ba5698e96b3015.jpg\" class=\"err-product\" height=\"402\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/130cac77991145099d6cbe622d43f999.jpg\" class=\"err-product\" height=\"490\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/a1d03da035b2406eb8e2b77bc5487400.jpg\" class=\"err-product\" height=\"689\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/e47222a476104aa884c865f8c1fc490f.jpg\" class=\"err-product\" height=\"609\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/c583d35aea6a4b45ad706aa39d4bf56b.jpg\" class=\"err-product\" height=\"636\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/073b566967db449986b4bc3cb465ba2f.jpg\" class=\"err-product\" height=\"542\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/721/remark/b2a4b69fdc17429d92810366cde37ff7.jpg\" class=\"err-product\" height=\"618\" width=\"750\"/></span></p><p><br/></p>'),(0,722,NULL,'<p></p><p><img class=\"err-product\" style=\"border: 0px; vertical-align: middle; color: rgb(51, 51, 51); font-family: Arial, 宋体; line-height: 18px; white-space: normal; background-image: url(&quot;http://res.suning.cn/public/v3/css/images/blankbg.gif?v=2015102601&quot;); background-color: rgb(255, 255, 255); background-position: 50% 50%; background-repeat: no-repeat;\" alt=\"\" src=\"/Storage/Shop/1/Products/722/remark/3bad2edd27264da6a9885c15761a39a8.jpg\"/><img class=\"err-product\" style=\"border: 0px; vertical-align: middle; color: rgb(51, 51, 51); font-family: Arial, 宋体; line-height: 18px; white-space: normal; background-image: url(&quot;http://res.suning.cn/public/v3/css/images/blankbg.gif?v=2015102601&quot;); background-color: rgb(255, 255, 255); background-position: 50% 50%; background-repeat: no-repeat;\" alt=\"\" src=\"/Storage/Shop/1/Products/722/remark/2be3d3af29aa4bc783eacdb8b662b5ff.jpg\"/><img class=\"err-product\" style=\"border: 0px; vertical-align: middle; color: rgb(51, 51, 51); font-family: Arial, 宋体; line-height: 18px; white-space: normal; background-image: url(&quot;http://res.suning.cn/public/v3/css/images/blankbg.gif?v=2015102601&quot;); background-color: rgb(255, 255, 255); background-position: 50% 50%; background-repeat: no-repeat;\" alt=\"\" src=\"/Storage/Shop/1/Products/722/remark/f691e51a102f4d9b9288cde1d956b192.jpg\"/><img class=\"err-product\" style=\"border: 0px; vertical-align: middle; color: rgb(51, 51, 51); font-family: Arial, 宋体; line-height: 18px; white-space: normal; background-image: url(&quot;http://res.suning.cn/public/v3/css/images/blankbg.gif?v=2015102601&quot;); background-color: rgb(255, 255, 255); background-position: 50% 50%; background-repeat: no-repeat;\" alt=\"\" src=\"/Storage/Shop/1/Products/722/remark/1f5592e429ea4d50aaea6a7454681962.jpg\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p></p><p><img class=\"err-product\" style=\"border: 0px; vertical-align: middle; color: rgb(51, 51, 51); font-family: Arial, 宋体; line-height: 18px; white-space: normal; background-image: url(&quot;http://res.suning.cn/public/v3/css/images/blankbg.gif?v=2015102601&quot;); background-color: rgb(255, 255, 255); background-position: 50% 50%; background-repeat: no-repeat;\" alt=\"\" src=\"/Storage/Shop/1/Products/722/remark/70b241cff0464c79b2408a013f4b60da.jpg\"/><img class=\"err-product\" style=\"border: 0px; vertical-align: middle; color: rgb(51, 51, 51); font-family: Arial, 宋体; line-height: 18px; white-space: normal; background-image: url(&quot;http://res.suning.cn/public/v3/css/images/blankbg.gif?v=2015102601&quot;); background-color: rgb(255, 255, 255); background-position: 50% 50%; background-repeat: no-repeat;\" alt=\"\" src=\"/Storage/Shop/1/Products/722/remark/d57da045c18f4c168a4123e301f8b005.jpg\"/><img class=\"err-product\" style=\"border: 0px; vertical-align: middle; color: rgb(51, 51, 51); font-family: Arial, 宋体; line-height: 18px; white-space: normal; background-image: url(&quot;http://res.suning.cn/public/v3/css/images/blankbg.gif?v=2015102601&quot;); background-color: rgb(255, 255, 255); background-position: 50% 50%; background-repeat: no-repeat;\" alt=\"\" src=\"/Storage/Shop/1/Products/722/remark/d0f2957fb31d4006b043942c4a93bcb0.jpg\"/><img class=\"err-product\" style=\"border: 0px; vertical-align: middle; color: rgb(51, 51, 51); font-family: Arial, 宋体; line-height: 18px; white-space: normal; background-image: url(&quot;http://res.suning.cn/public/v3/css/images/blankbg.gif?v=2015102601&quot;); background-color: rgb(255, 255, 255); background-position: 50% 50%; background-repeat: no-repeat;\" alt=\"\" src=\"/Storage/Shop/1/Products/722/remark/7455cb018c4a4305bd5ff2651b8870bc.jpg\"/></p><p><br/></p>'),(0,723,NULL,'<p>	</p><p>\n		\n	\n	\n	</p><p>\n			</p><p><img alt=\"\" src=\"/Storage/Shop/1/Products/723/remark/bf70d8dda6724e8da5f91b6fd5581118.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/723/remark/e94cd18f26544a03a9fb08459423327a.jpg\" class=\"err-product\" height=\"981\" width=\"750\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p>	</p><p>\n		\n	\n	\n	</p><p>\n			</p><p><img alt=\"\" src=\"/Storage/Shop/1/Products/723/remark/58bff95373dd4461baf87c1bfe2a48f1.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/723/remark/f5f2a23014ed4f0286fce9de299e50ed.jpg\" class=\"err-product\" height=\"981\" width=\"750\"/></p><p><br/></p>'),(0,724,'不符合商品发布要求','<p><img src=\"/Storage/Shop/1/Products/724/remark/5193b48188814ba98833bc8e296b3c36.jpg\" style=\"\" title=\"109410844010947290987500_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/724/remark/10de9784139b40bfac390029ec57e778.jpg\" style=\"\" title=\"168810314838304023352690_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/724/remark/cef1b478c146467bacb481830f41c7ca.jpg\" style=\"\" title=\"189017562165177084894720_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/724/remark/b822f25a16f84277a26e9962146ab01c.jpg\" style=\"\" title=\"200264111113504944268818_x.jpg\"/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/724/remark/1fe069b4d1ce4dbeaf060d936f21352b.jpg\" style=\"\" title=\"109410844010947290987500_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/724/remark/c29709f0071e401c920f8a27ebf72035.jpg\" style=\"\" title=\"168810314838304023352690_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/724/remark/1ee248f63c214355969e971da38c31f8.jpg\" style=\"\" title=\"189017562165177084894720_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/724/remark/c0f46be6da1549dfb71f728e7112e83c.jpg\" style=\"\" title=\"200264111113504944268818_x.jpg\"/></p>'),(0,725,NULL,'<p><img src=\"/Storage/Shop/1/Products/725/remark/c4097f57fc424e3d86e754500a4619ba.jpg\" style=\"\" title=\"109410844010947290987500_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/725/remark/91fd2b27d8364b2dbb18d6706ba78d0d.jpg\" style=\"\" title=\"168810314838304023352690_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/725/remark/6500c4f38f7c448cab6ff5e141ae7b16.jpg\" style=\"\" title=\"189017562165177084894720_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/725/remark/4c3fbdcfefc74abab68e49b8b0316196.jpg\" style=\"\" title=\"200264111113504944268818_x.jpg\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/725/remark/71403aee09fd4c9caddd19167b424e5f.jpg\" style=\"\" title=\"109410844010947290987500_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/725/remark/ebdec17515db4400bdfdd79b76642587.jpg\" style=\"\" title=\"168810314838304023352690_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/725/remark/cfe8bcc45ff24dc1babe184028921562.jpg\" style=\"\" title=\"189017562165177084894720_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/725/remark/710bcf3455f84129abbbc1043c562de4.jpg\" style=\"\" title=\"200264111113504944268818_x.jpg\"/></p><p><br/></p>'),(0,726,NULL,'<p><img src=\"/Storage/Shop/1/Products/726/remark/4afbb713356b499885b16d24c3c1952a.jpg\" style=\"\" title=\"116200978242180899512160_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/726/remark/d516ce2cc1c4433a87fee83ae5b5b26e.jpg\" style=\"\" title=\"242294035163130769032200_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/726/remark/adce8ffeca5441a4b405378dcfc8f8ff.jpg\" style=\"\" title=\"404022205594154706214500_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/726/remark/b3a235f1dc644d339aa295144db793b7.jpg\" style=\"\" title=\"568878251566822860945400_x.jpg\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/726/remark/aaf43fb5a6044e90a37a78b9c2086fb2.jpg\" style=\"\" title=\"116200978242180899512160_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/726/remark/debc9be8ea3041d5ac11f6464482a23f.jpg\" style=\"\" title=\"242294035163130769032200_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/726/remark/ad367edcea784e5286b8d6803fa0dfc4.jpg\" style=\"\" title=\"404022205594154706214500_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/726/remark/89c31d75a552472897ac31225fe5c3ee.jpg\" style=\"\" title=\"568878251566822860945400_x.jpg\"/></p><p><br/></p>'),(0,727,NULL,'<p><img alt=\"\" src=\"/Storage/Shop/1/Products/727/remark/2ddd4acbf5b7485d9b210cea10382645.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/727/remark/585d6b6c1490404487122be39387835d.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/727/remark/3c72ab010d504ddca8dd192cc474985b.jpg\" class=\"err-product\"/></p><p><img alt=\"\" src=\"/Storage/Shop/1/Products/727/remark/5860d6637ed4462fa8ac9be632f7e36f.jpg\" class=\"err-product\"/></p><p><img alt=\"\" src=\"/Storage/Shop/1/Products/727/remark/da94d1c49ab1480bbcf1e2c0fbc45026.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/727/remark/08be69df80d54af0a62ba66a8ca276b8.jpg\" class=\"err-product\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p><img alt=\"\" src=\"/Storage/Shop/1/Products/727/remark/59c40726815548bb9c19889e0985b1e3.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/727/remark/905c7b51ea234ea2b4b738969e2153cc.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/727/remark/bbaa0d28f21e46ec91e07ec4eab1dee7.jpg\" class=\"err-product\"/></p><p><img alt=\"\" src=\"/Storage/Shop/1/Products/727/remark/a962a1613dac44f282868aadcfd28b49.jpg\" class=\"err-product\"/></p><p><img alt=\"\" src=\"/Storage/Shop/1/Products/727/remark/abd0eee467e04946aaa13dd19e72d0f8.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/727/remark/cb785cba7b0b4e71acb44175d60ba42e.jpg\" class=\"err-product\"/></p><p><br/></p>'),(0,728,NULL,'<p><br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/4a68826fc97d43bea1cf8b3e0e557a99.jpg\" id=\"34141639bb7646649cd6d83221194ec4\n\"/> \n &nbsp;<br/> \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/132fed4193694a999a6e8fe8db099741.jpg\" id=\"8b6bbdc8c78a4ab0a110f0da90499519\n\"/>&nbsp;&nbsp; \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/f59de7e2aac14e47bd164144184fc777.jpg\" id=\"ec599c9b75c0476a91e0be6d24f4c5f7\n\"/> \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/7e8eb6979a4c45df9fbdae32b9925059.jpg\" id=\"d50d13a213f94ef68c21f5fedeccc6df\n\"/> \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/d4a115d8508b436094faae9d4c6445b2.jpg\" id=\"386461b85e7c4dd585008c69f5204732\n\"/> \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/f6e08a6b7c604a0ca93cd1989331fbe3.jpg\" id=\"3cb5315ff91d4321ac6b2909cfc03bb8\n\"/> \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/af92cd27128341d88f91eedb90dc96ec.jpg\" id=\"0af8e2bc4e784b379a3daf2bf0530224\n\"/> \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/4a6e8b3642ca450881bb79a24d8bbe27.jpg\" id=\"7b4616d0677d4dafbeb2aacba3dd341d\n\"/> \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/88e757b95e0847fab684ecece3b30c9d.jpg\" id=\"db39f6be7d3243dab0e3c6bb2fc25dd0\n\"/> \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/25f0bce1f4204834933a9cde4400ef0f.jpg\" id=\"ee6fb2d1a4ad4d64ae16d9bede615a4b\n\"/></p>',0,0,NULL,NULL,NULL,'<p><br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/175b17b6d2e14d9ea7a5ab6c32f63f2a.jpg\" id=\"34141639bb7646649cd6d83221194ec4\n\"/> \n &nbsp;<br/> \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/cb0ec8b5ffe9487db1d8c66eeeff8ffe.jpg\" id=\"8b6bbdc8c78a4ab0a110f0da90499519\n\"/>&nbsp;&nbsp; \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/7464639d158f406bb0e23a894a90c31c.jpg\" id=\"ec599c9b75c0476a91e0be6d24f4c5f7\n\"/> \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/2159dc285716426db86ac33ff6287296.jpg\" id=\"d50d13a213f94ef68c21f5fedeccc6df\n\"/> \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/594de231bdd34853bb5da4582444ba06.jpg\" id=\"386461b85e7c4dd585008c69f5204732\n\"/> \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/54048e0c6e184dc89efb57f7a5c87877.jpg\" id=\"3cb5315ff91d4321ac6b2909cfc03bb8\n\"/> \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/36b6febe7e80489190dcb15d15dfab22.jpg\" id=\"0af8e2bc4e784b379a3daf2bf0530224\n\"/> \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/11301b2855d2486ea96b6d0ee6f6f8cd.jpg\" id=\"7b4616d0677d4dafbeb2aacba3dd341d\n\"/> \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/eb293544b2844a40859246e42236d593.jpg\" id=\"db39f6be7d3243dab0e3c6bb2fc25dd0\n\"/> \n &nbsp;<br/> \n &nbsp;<img src=\"/Storage/Shop/1/Products/728/remark/17fa3ab046b8422ca879da336df230a9.jpg\" id=\"ee6fb2d1a4ad4d64ae16d9bede615a4b\n\"/></p>'),(0,729,NULL,'<p style=\"text-align: center;\"><br/></p><table id=\"Table_01\" cellspacing=\"0\" cellpadding=\"0\" width=\"750\"><tbody><tr class=\"firstRow\"></tr></tbody></table><p>&nbsp;</p><p></p><p></p><p></p><p></p><p>\n			</p><p><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/3ca2c81e167f43c9b291fec5c5ac7c4c.jpg\" class=\"err-product\" height=\"1125\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/ed91dc1ed6624915b2cd5643e31f53c1.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/daa4c020668846118777398a54ccb916.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/f24eeea4ab174b75a728ed58b09bf535.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/078a7f011ce54a1582eef807fc93ada4.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/9368c37713594473a636575c63600bd9.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/726dba6381e1446ca488583007e519b7.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/44cebf8a35b04d35b330f309d66a89a8.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/77edc94b83b64606be6acb624f479b57.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/bb683195bdca46b78bc932a90e7b17b3.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/6966e8ed0f7e4ba4af5b5acc1a39403d.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/b11c808fc20f4bd6baaad2e83f6d73f4.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/6daaf5f63e9d49359f36fd1c6d9c9f1a.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/400c1509430b463881678e2488e684d5.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/8814bc00c6b84508b42f7d47732c204c.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/0ac0daf229bd441aab564744079c58cc.jpg\" class=\"err-product\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p style=\"white-space: normal;\"><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/757950bd0a8040b9a43e8ae07a3a2c06.jpg\" class=\"err-product\" height=\"1125\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/0456ad99af804e68af25dc8ede5463ba.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/7993d982d2574050b3af48281689355d.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/9cdb01733fd6438bb3d28fe5f9cafbac.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/272fca1d0b16461fb677d0383eef8620.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/c843f4ab33364e5eaa802d82ad337c1b.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/7bb2ec6ea6984dadac5c16d51b78bc21.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/378e8306ee974cc698bf501296008f1b.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/9515a1086a3f4b5f8347adbbbdccc094.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/bc109e6eec764927a02f65db09d53ba1.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/d81cbc6abfff472b8471355e633fe556.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/9686c0fbfc3343299c0f970135f4a69b.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/78642f0c6e5e424bbe6dab85f9e620ad.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/67700b6a80494f16a61bff6c7bc64da8.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/2579ff06869e43c9a62f10dde016d3fb.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/729/remark/deecdf067ead4a45ad75dde35cc340db.jpg\" class=\"err-product\"/></p><p><br/></p>'),(0,730,NULL,'<p style=\"text-align: center;\"><br/></p><table id=\"Table_01\" cellspacing=\"0\" cellpadding=\"0\" width=\"750\"><tbody><tr class=\"firstRow\"></tr></tbody></table><p>&nbsp;</p><p style=\"white-space: normal;\"><img alt=\"\" src=\"/Storage/Shop/1/Products/730/remark/8dc46bc1fb984fa2b31ddd365a25800d.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/730/remark/f6424d96899b4538928dc75a50db31ce.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/730/remark/2d98ccd2ff5b43b39af2ac7a9e2ec00e.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/730/remark/f873f08d43e44df6bb68e3b7e2a4948d.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/730/remark/c3314e584dfd44dca15e8d5ae5539b19.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/730/remark/ae91971498b7428f9740f864dbb3c6a8.jpg\" class=\"err-product\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p style=\"white-space: normal;\"><img alt=\"\" src=\"/Storage/Shop/1/Products/730/remark/039e860ca67c4cea8a3467830630cf9f.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/730/remark/97566ca2b4b64c1291e68712965428f9.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/730/remark/ba2ad22da4154b9b913e01ad275c5974.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/730/remark/ac6a9dfa524c4ef0a3c584a0d25c6458.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/730/remark/3607ec7dfb924093a949e4ded2403c35.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/730/remark/a6d916f8a81c4033a362ddd8e444278c.jpg\" class=\"err-product\"/></p><p><br/></p>'),(0,731,NULL,'<p><img src=\"/Storage/Shop/1/Products/731/remark/29f8e4f74eb84abfbe6567dc3bd7c006.jpg\" style=\"\" title=\"209554334197123352980250_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/731/remark/9f0311eb387a4bbbae87f14d58840eec.jpg\" style=\"\" title=\"149619526513049839483124_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/731/remark/cd6a92db45ac4c8390b9b1c06cd5ca5b.jpg\" style=\"\" title=\"153644230497709383740290_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/731/remark/fe2ca3d4b6f34deab39d27d09d2c6d7c.jpg\" style=\"\" title=\"159367532318614918178614_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/731/remark/000da6a94097480f93bf36f3a5b1a0b8.jpg\" style=\"\" title=\"194235861660112372672400_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/731/remark/83a0a3301f4f4fb6affa42aee0f3dd97.jpg\" style=\"\" title=\"527900898580223302351000_x.jpg\"/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/731/remark/6b5934dcb4bf450e9c7a43003d32bb65.jpg\" style=\"\" title=\"209554334197123352980250_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/731/remark/d077ed1080d944e19ff2f69b0bb37a62.jpg\" style=\"\" title=\"243699664793479767878100_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/731/remark/5a491bed275b400ead7e721f1618804b.jpg\" style=\"\" title=\"527900898580223302351000_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/731/remark/745bd62b00c44df7925b9f8ae2ed2952.jpg\" style=\"\" title=\"149619526513049839483124_x.jpg\"/></p>'),(0,732,NULL,'<p><img src=\"/Storage/Shop/1/Products/732/remark/bdffdd7fc8fa4843b8b09b968dd21c56.jpg\" style=\"\" title=\"195002464423249705883500_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/732/remark/22c39d59e0484171a29f862df8ddc6ef.jpg\" style=\"\" title=\"614524568244819835976000_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/732/remark/5155078390e146ee96a5f885ea693ba7.jpg\" style=\"\" title=\"143048838312642273387230_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/732/remark/d4702ac6d38248038ff49a99985816ac.jpg\" style=\"\" title=\"195002464423249705883500_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/732/remark/d488f888eb87402ab6f525703e3e02f7.jpg\" style=\"\" title=\"128055877307585846689100_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/732/remark/8a826f60622846a3820aa5fe9f1d7ca7.jpg\" style=\"\" title=\"532910300126203185575220_x.jpg\"/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/732/remark/897d11bf97ca41a0946d03362164b1a7.jpg\" style=\"\" title=\"195002464423249705883500_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/732/remark/3f70d10e51254f138a8165649bb62934.jpg\" style=\"\" title=\"614524568244819835976000_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/732/remark/c0048f0e234648ebb7d38c2b7d523a21.jpg\" style=\"\" title=\"143048838312642273387230_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/732/remark/6331b35cfad4487e806d59f5fdb6bbb5.jpg\" style=\"\" title=\"128055877307585846689100_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/732/remark/9340224d54bb45b1a2944bfe39734f32.jpg\" style=\"\" title=\"532910300126203185575220_x.jpg\"/></p>'),(0,733,NULL,'<p><img src=\"/Storage/Shop/1/Products/733/remark/7e7f86e7a31b4ef89be411a2aaa8a828.jpg\" title=\"605303069192223693765340_x.jpg\" alt=\"605303069192223693765340_x.jpg\"/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/733/remark/97ae97b480ba4ff6b83cf39c79c55b5a.jpg\" title=\"605303069192223693765340_x.jpg\" alt=\"605303069192223693765340_x.jpg\"/></p>'),(0,734,NULL,'<p><img src=\"/Storage/Shop/1/Products/734/remark/2df37f1c830d4922a4388592712b9263.jpg\" style=\"\" title=\"209284042610678409187503_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/734/remark/22030a54016b49ffaebe1419e3860a94.jpg\" style=\"\" title=\"155405307428358877855210_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/734/remark/195616c83d9a4737888d302105d0559d.jpg\" style=\"\" title=\"993928675280359254363800_x.jpg\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/734/remark/0c8fca7f70924ace856e4e2e2c655d2e.jpg\" style=\"\" title=\"209284042610678409187503_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/734/remark/fbedc854364540848608ed8752ecf405.jpg\" style=\"\" title=\"155405307428358877855210_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/734/remark/de1bf5f2e4474a9cb386ca73e9227705.jpg\" style=\"\" title=\"993928675280359254363800_x.jpg\"/></p><p><br/></p>'),(0,735,NULL,'<p><img src=\"/Storage/Shop/1/Products/735/remark/c20d18dd8ce44d18ad82a8b089366c2c.jpg\" style=\"\" title=\"132947138174987279773260_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/735/remark/61804bcef0114152906e1073a7e948fb.jpg\" style=\"\" title=\"434204117604219104195200_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/735/remark/b9005543099746e3b51769614e4e9a7a.jpg\" style=\"\" title=\"545816566120000764139960_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/735/remark/c6f1e6ea27fe4c1caf59492875a456d6.jpg\" style=\"\" title=\"290564507192393574294180_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/735/remark/f373fffe3703433d8bf662ef83dabd3b.jpg\" style=\"\" title=\"174581541815330709731594_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/735/remark/be3d243cd9d743c5ac95da69e85ca4ef.jpg\" style=\"\" title=\"109018166285452710015770_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/735/remark/b008b011daa744ceb8d25c64a12ddc16.jpg\" style=\"\" title=\"000000000145086165_5.jpg\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/735/remark/e45f945a40b344e692e922ed4bd42ec6.jpg\" style=\"\" title=\"132947138174987279773260_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/735/remark/cdd6014e85bc46b585050d91076c499b.jpg\" style=\"\" title=\"174581541815330709731594_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/735/remark/057ed9f3ef104b08b3f4670ab9151386.jpg\" style=\"\" title=\"290564507192393574294180_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/735/remark/1eeefb8c61bd48deac2741335c4b88cf.jpg\" style=\"\" title=\"000000000145086165_5.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/735/remark/c7a2f1b42f7544f58e53b20456a6038b.jpg\" style=\"\" title=\"545816566120000764139960_x.jpg\"/></p><p><br/></p>'),(0,736,NULL,'<p><br/></p><p><br/></p><p><br/></p><p><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/8ceaade463b740b69efe3beb04619261.jpg\" class=\"err-product\" height=\"518\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/5c417e47760b4de2b3687d7b18298f03.jpg\" class=\"err-product\" height=\"383\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/9db4eaed5fc04a04aa44028a3781c36b.jpg\" class=\"err-product\" height=\"442\" width=\"750\"/></p><p style=\"text-align: center;\"><span style=\"font-size: 18px;\"><strong>新老包装 随机发放</strong></span><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/3b5956c5fdc142b3aaa7a3643f1439a9.jpg\" class=\"err-product\" height=\"633\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/f2a858a6135d45a08de3b2d7b0fbb2a2.jpg\" class=\"err-product\" height=\"562\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/536240af920c4ae7b6ec7fd85d2ef20a.jpg\" class=\"err-product\" height=\"493\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/645ee5dad898498598240e31e33f67da.jpg\" class=\"err-product\" height=\"495\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/64da81241b8e4dc7915274e8e84fc00d.jpg\" class=\"err-product\" height=\"530\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/a6b55c98e8ab484ba39d3f7d15ea2bec.jpg\" class=\"err-product\" height=\"843\" width=\"750\"/></p>',0,0,NULL,NULL,NULL,'<p><br/></p><p><br/></p><p><br/></p><p><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/a488db2c81d544cabb3a5d5c24cfcdbf.jpg\" class=\"err-product\" height=\"518\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/5afa433d57974bf881561d2de55327e3.jpg\" class=\"err-product\" height=\"383\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/c165a125b251482bbc42c95ae35fe1f3.jpg\" class=\"err-product\" height=\"442\" width=\"750\"/></p><p style=\"text-align: center;\"><span style=\"font-size: 18px;\"><strong>新老包装 随机发放</strong></span><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/c9938e688ce5467e94389a01b5f0a722.jpg\" class=\"err-product\" height=\"633\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/ecf6dce7344b47c69f2d06ff2cd41580.jpg\" class=\"err-product\" height=\"562\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/5ec34527b32b4fc0a7903a459a8a2887.jpg\" class=\"err-product\" height=\"493\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/30e1de2a4719442d83766b54500970fc.jpg\" class=\"err-product\" height=\"495\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/0b1e2fcec71d4d2cba819f1e2ac9f601.jpg\" class=\"err-product\" height=\"530\" width=\"750\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/736/remark/105165cb34124ab2a0acbd1b3bdec2a1.jpg\" class=\"err-product\" height=\"843\" width=\"750\"/></p>'),(0,737,NULL,'<p><img src=\"/Storage/Shop/1/Products/737/remark/2596b3f7a0cf49078448670f782ce972.jpg\" title=\"632797017101150214225850_x.jpg\" alt=\"632797017101150214225850_x.jpg\" width=\"605\" height=\"212\" style=\"width: 605px; height: 212px;\"/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/737/remark/c3a81f49a10b4dcc9f030928bb2b0908.jpg\" title=\"632797017101150214225850_x.jpg\" alt=\"632797017101150214225850_x.jpg\" width=\"529\" height=\"212\" style=\"width: 529px; height: 212px;\"/></p>'),(0,738,NULL,'<ul class=\"cnt clearfix list-paddingleft-2\"><li><p><br/></p></li><li><p>品牌：<a href=\"http://www.suning.com/pinpai/7131-0-0.html\" target=\"_blank\">妮维雅</a></p></li><li><p><br/></p></li><li><p>类别：洁肤水</p></li><li><p><br/></p></li><li><p>适合肤质：各种肤质</p></li><li><p><br/></p></li><li><p>适用人群：男士</p></li><li><p><br/></p></li><li><p>规格：150ml</p></li><li><p><br/></p></li><li><p>功效：保湿补水,深层清洁</p></li><li><p><img title=\"苏宁图片.png\" alt=\"苏宁图片.png\" src=\"/Storage/Shop/1/Products/738/remark/29739ea3851f40e697865e8760ae3418.png\" class=\"err-product\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/></p><p><br/></p></li></ul><p>&nbsp;</p><p style=\"text-align: center;\">&nbsp;（新老包装 随机发放）</p><p>&nbsp;</p><p><img style=\"display: block; margin-left: auto; margin-right: auto;\" title=\"suningtupian -1.png\" alt=\"suningtupian -1.png\" src=\"/Storage/Shop/1/Products/738/remark/e170cde01d32409dada08ad1e2aa5cbe.png\" class=\"err-product\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<ul class=\"cnt clearfix list-paddingleft-2\"><li><p></p></li><li><p>品牌：<a href=\"http://www.suning.com/pinpai/7131-0-0.html\" target=\"_blank\">妮维雅</a></p></li><li><p></p></li><li><p>类别：洁肤水</p></li><li><p></p></li><li><p>适合肤质：各种肤质</p></li><li><p></p></li><li><p>适用人群：男士</p></li><li><p></p></li><li><p>规格：150ml</p></li><li><p></p></li><li><p>功效：保湿补水,深层清洁</p></li><li><p></p></li></ul><p>	</p><p>\n		\n	\n	\n	</p><p>\n			</p><p><img title=\"苏宁图片.png\" alt=\"苏宁图片.png\" src=\"/Storage/Shop/1/Products/738/remark/ee2f6edae26f4add85d0a34260ccba95.png\" class=\"err-product\"/></p><p>&nbsp;</p><p style=\"text-align: center;\">&nbsp;（新老包装 随机发放）</p><p>&nbsp;</p><p><img style=\"display: block; margin-left: auto; margin-right: auto;\" title=\"suningtupian -1.png\" alt=\"suningtupian -1.png\" src=\"/Storage/Shop/1/Products/738/remark/f6a6ab653ecc441aaefd3e3c93fd5594.png\" class=\"err-product\"/></p><p><br/></p>'),(0,739,NULL,'<h4 class=\"txt-head\"></h4><ul class=\"clearfix list-paddingleft-2\"><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>核心数</strong>：双核心</p></li><li><p><strong>CPU类型</strong>：Intel i5</p></li><li><p><strong>CPU型号</strong>：i5-4210H</p></li><li><p><strong>CPU主频</strong>：2.9</p></li><li><p><em class=\"icon i-cpu\"></em></p><p>CPU</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>硬盘容量</strong>：1TB</p></li><li><p><strong>硬盘类型</strong>：机械硬盘</p></li><li><p><strong>硬盘转速</strong>：5400转/分</p></li><li><p><em class=\"icon i-pc-dish\"></em></p><p>硬盘</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>内存容量</strong>：4GB</p></li><li><p><strong>内存类型</strong>：DDR3</p></li><li><p><strong>最大支持内存</strong>：16GB</p></li><li><p><em class=\"icon i-pc-memory\"></em></p><p>内存</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>显卡类型</strong>：独立显卡</p></li><li><p><strong>显存容量</strong>：2GB</p></li><li><p><em class=\"icon i-pc-graphic\"></em></p><p>显卡</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>屏幕尺寸</strong>：15.6英寸</p></li><li><p><strong>屏幕比例</strong>：16:9</p></li><li><p><strong>屏幕分辨率</strong>：1920×1080</p></li><li><p><strong>触摸功能</strong>：不支持</p></li><li><p><em class=\"icon i-pc-screen\"></em></p><p>显示屏</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>颜色</strong>：黑色</p></li><li><p><strong>厚度</strong>：23.9mm</p></li><li><p><strong>机身尺寸</strong>：387 * 263.4 * 23.9</p></li><li><p><strong>重量</strong>：2.6KG</p></li><li><p><em class=\"icon i-pc-appearance\"></em></p><p>外观</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>电池类型</strong>：4芯锂电池</p></li><li><p><strong>续航时间</strong>：适使用情况而定</p></li><li><p><strong>电源适配器</strong>：100-240V自适应交流电源适配器</p></li><li><p><em class=\"icon i-pc-battery\"></em></p><p>电池</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>操作系统</strong>：Windows8.1</p></li><li><p><strong>蓝牙功能</strong>：蓝牙4.0</p></li><li><p><strong>局域网</strong>：10/100/1000Mbps</p></li><li><p><strong>无线局域网</strong>：802.11b/g/n</p></li><li><p><em class=\"icon i-others\"></em></p><p>其他</p></li></ul><p><br/></p></ul><table cellspacing=\"1\" cellpadding=\"1\" width=\"750\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td>&nbsp;<img alt=\"\" src=\"/Storage/Shop/1/Products/739/remark/8c076ee49295465f9d4e1de6af406daf.jpg\" class=\"err-product\"/></td></tr></tbody></table><table cellspacing=\"1\" cellpadding=\"1\" width=\"750\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td><p><img alt=\"\" src=\"/Storage/Shop/1/Products/739/remark/9a743ea73d98494d96cf64e03285c01d.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/739/remark/22f5eea2df484daf8b49382f65bbc9b3.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/739/remark/04878cd1a8754234b27b49cd25b9d952.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/739/remark/e5be3cc95e81469a81aa293af6763718.jpg\" class=\"err-product\"/></p></td></tr></tbody></table><p><br/></p>',0,0,NULL,NULL,NULL,'<ul class=\"clearfix list-paddingleft-2\" style=\"width: 608.938px; white-space: normal;\"><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>核心数</strong>：双核心</p></li><li><p><strong>CPU类型</strong>：Intel i5</p></li><li><p><strong>CPU型号</strong>：i5-4210H</p></li><li><p><strong>CPU主频</strong>：2.9</p></li><li><p><em class=\"icon i-cpu\"></em></p><p>CPU</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p></li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>硬盘容量</strong>：1TB</p></li><li><p><strong>硬盘类型</strong>：机械硬盘</p></li><li><p><strong>硬盘转速</strong>：5400转/分</p></li><li><p><em class=\"icon i-pc-dish\"></em></p><p>硬盘</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p></li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>内存容量</strong>：4GB</p></li><li><p><strong>内存类型</strong>：DDR3</p></li><li><p><strong>最大支持内存</strong>：16GB</p></li><li><p><em class=\"icon i-pc-memory\"></em></p><p>内存</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p></li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>显卡类型</strong>：独立显卡</p></li><li><p><strong>显存容量</strong>：2GB</p></li><li><p><em class=\"icon i-pc-graphic\"></em></p><p>显卡</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p></li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>屏幕尺寸</strong>：15.6英寸</p></li><li><p><strong>屏幕比例</strong>：16:9</p></li><li><p><strong>屏幕分辨率</strong>：1920×1080</p></li><li><p><strong>触摸功能</strong>：不支持</p></li><li><p><em class=\"icon i-pc-screen\"></em></p><p>显示屏</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p></li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>颜色</strong>：黑色</p></li><li><p><strong>厚度</strong>：23.9mm</p></li><li><p><strong>机身尺寸</strong>：387 * 263.4 * 23.9</p></li><li><p><strong>重量</strong>：2.6KG</p></li><li><p><em class=\"icon i-pc-appearance\"></em></p><p>外观</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p></li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>电池类型</strong>：4芯锂电池</p></li><li><p><strong>续航时间</strong>：适使用情况而定</p></li><li><p><strong>电源适配器</strong>：100-240V自适应交流电源适配器</p></li><li><p><em class=\"icon i-pc-battery\"></em></p><p>电池</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p></li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>操作系统</strong>：Windows8.1</p></li><li><p><strong>蓝牙功能</strong>：蓝牙4.0</p></li><li><p><strong>局域网</strong>：10/100/1000Mbps</p></li><li><p><strong>无线局域网</strong>：802.11b/g/n</p></li><li><p><em class=\"icon i-others\"></em></p><p>其他</p></li></ul><p><br/></p></ul><table cellspacing=\"1\" cellpadding=\"1\" width=\"625\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td>&nbsp;<img alt=\"\" src=\"/Storage/Shop/1/Products/739/remark/5c2ad565822d47cc8b419cfbbd4331ac.jpg\" class=\"err-product\"/></td></tr></tbody></table><table cellspacing=\"1\" cellpadding=\"1\" width=\"625\" style=\"width: 625px;\"><tbody><tr class=\"firstRow\"><td><p><img alt=\"\" src=\"/Storage/Shop/1/Products/739/remark/57ce3edf9d0543768c5df577ab6d0bf2.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/739/remark/397a93426a2f4de58e142abce3a605eb.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/739/remark/b7ba26f1d840470dabbc4400cc0db371.jpg\" class=\"err-product\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/739/remark/e509d89df0df4745915320e27000f67c.jpg\" class=\"err-product\"/></p></td></tr></tbody></table><p><br/></p>'),(0,740,NULL,'<p><img src=\"/Storage/Shop/1/Products/740/remark/5ffd2c2df2be42beaf7c587a9e75576d.jpg\" style=\"\" title=\"196633272310882068712202_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/740/remark/14c2d210c663435ab03277c45a462003.jpg\" style=\"\" title=\"202090685639838752997510_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/740/remark/e5ac03eed0504e2aa912db79fa16f309.jpg\" style=\"\" title=\"174286217717460816478252_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/740/remark/acfedb88ff4d487a99af189830679e66.jpg\" style=\"\" title=\"175749673882711291510780_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/740/remark/c3a478ecd0a04ea9babecdf219b47b08.jpg\" style=\"\" title=\"610859365137508236086940_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/740/remark/f3369a23a31248d5a08c8081217705ca.jpg\" style=\"\" title=\"207511908627900835665620_x.jpg\"/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/740/remark/7229756a557d432fa4a423a77fb2c203.jpg\" style=\"\" title=\"196633272310882068712202_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/740/remark/3ff631db46144439b71515bcac49989f.jpg\" style=\"\" title=\"202090685639838752997510_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/740/remark/24537d1273674c2fba2cefe2d13ca1f3.jpg\" style=\"\" title=\"174286217717460816478252_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/740/remark/314f678a46d24e2e97abb9a51c668e4e.jpg\" style=\"\" title=\"175749673882711291510780_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/740/remark/d1dc41b943064c49af7c3ed3afc0cb05.jpg\" style=\"\" title=\"610859365137508236086940_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/740/remark/9ac0bc18f2a3446882224832ccef9621.jpg\" style=\"\" title=\"207511908627900835665620_x.jpg\"/></p>'),(0,741,NULL,'<p>\n	</p><h4 class=\"txt-head\">核心参数</h4><ul class=\"clearfix list-paddingleft-2\"><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>核心数</strong>：四核心</p></li><li><p><strong>CPU类型</strong>：Intel i7</p></li><li><p><strong>CPU型号</strong>：i7-6700HQ</p></li><li><p><strong>CPU主频</strong>：2.6GHZ</p></li><li><p><em class=\"icon i-cpu\"></em></p><p>CPU</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>硬盘容量</strong>：1TB</p></li><li><p><strong>硬盘类型</strong>：机械硬盘</p></li><li><p><strong>硬盘转速</strong>：5400转/分</p></li><li><p><strong>硬盘接口类型</strong>：SATA接口</p></li><li><p><em class=\"icon i-pc-dish\"></em></p><p>硬盘</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>内存容量</strong>：8GB</p></li><li><p><strong>内存类型</strong>：DDR4</p></li><li><p><strong>最大支持内存</strong>：16GB</p></li><li><p><em class=\"icon i-pc-memory\"></em></p><p>内存</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>显卡类型</strong>：独立显卡</p></li><li><p><strong>显存容量</strong>：2GB</p></li><li><p><strong>显卡型号</strong>：NVIDIA® GeForce® GTX 950M 独立显示芯片</p></li><li><p><em class=\"icon i-pc-graphic\"></em></p><p>显卡</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>屏幕尺寸</strong>：15.6英寸</p></li><li><p><strong>屏幕比例</strong>：16:9</p></li><li><p><strong>屏幕分辨率</strong>：1920×1080</p></li><li><p><strong>屏幕类型</strong>：FHD背光</p></li><li><p><strong>触摸功能</strong>：不支持</p></li><li><p><em class=\"icon i-pc-screen\"></em></p><p>显示屏</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>颜色</strong>：红黑</p></li><li><p><strong>厚度</strong>：29.2mm</p></li><li><p><strong>机身尺寸</strong>：380*251*29.2mm</p></li><li><p><strong>重量</strong>：2.45</p></li><li><p><em class=\"icon i-pc-appearance\"></em></p><p>外观</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>电池类型</strong>：4芯锂电池</p></li><li><p><strong>续航时间</strong>：视使用环境而定</p></li><li><p><strong>电源适配器</strong>：输出：直流19 伏, 6.32 安, 120 瓦，输入：交流100 -240 伏，50/60 Hz</p></li><li><p><em class=\"icon i-pc-battery\"></em></p><p>电池</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>操作系统</strong>：Windows10</p></li><li><p><strong>蓝牙功能</strong>：蓝牙4.0</p></li><li><p><strong>局域网</strong>：10/100/1000Mbps</p></li><li><p><strong>无线局域网</strong>：802.11b/g/n</p></li><li><p><em class=\"icon i-others\"></em></p><p>其他</p></li><li><p><em class=\"ng-iconfont p-down-arrow\" style=\"font-family: sans-serif; font-size: 16px;\"><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/530a51f7ac5548fb83b671b7bfbf76d0.jpg\" class=\"err-product\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/0a44921c0d5f4254afbd30118df5c9a0.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/ff1a3a4ca81044e98e1d517bdff01702.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/a6c2a39ff8314b8eb1d929b2c88b5bf5.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/0c085c09fc944acfa5e7965facc12f9f.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/939fa30fca084ce8833da28375edceca.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/c7a03cad3b634744bf152afbb845c18c.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/04d759d54b8741b28237eef7dcde7807.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/b85c85b4332d4fff9bbc041a1882d9a7.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/a13c5f0567234c7bb5c14d934908c7a5.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/562c64fe8bf44bb3bd51d74c58df31a0.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/></em><br/></p></li></ul></ul><p><br/></p>',0,0,NULL,NULL,NULL,'<h4 class=\"txt-head\" style=\"white-space: normal;\">核心参数</h4><ul class=\"clearfix list-paddingleft-2\" style=\"width: 608.938px; white-space: normal;\"><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>核心数</strong>：四核心</p></li><li><p><strong>CPU类型</strong>：Intel i7</p></li><li><p><strong>CPU型号</strong>：i7-6700HQ</p></li><li><p><strong>CPU主频</strong>：2.6GHZ</p></li><li><p><em class=\"icon i-cpu\"></em></p><p>CPU</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p></li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>硬盘容量</strong>：1TB</p></li><li><p><strong>硬盘类型</strong>：机械硬盘</p></li><li><p><strong>硬盘转速</strong>：5400转/分</p></li><li><p><strong>硬盘接口类型</strong>：SATA接口</p></li><li><p><em class=\"icon i-pc-dish\"></em></p><p>硬盘</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p></li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>内存容量</strong>：8GB</p></li><li><p><strong>内存类型</strong>：DDR4</p></li><li><p><strong>最大支持内存</strong>：16GB</p></li><li><p><em class=\"icon i-pc-memory\"></em></p><p>内存</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p></li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>显卡类型</strong>：独立显卡</p></li><li><p><strong>显存容量</strong>：2GB</p></li><li><p><strong>显卡型号</strong>：NVIDIA® GeForce® GTX 950M 独立显示芯片</p></li><li><p><em class=\"icon i-pc-graphic\"></em></p><p>显卡</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p></li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>屏幕尺寸</strong>：15.6英寸</p></li><li><p><strong>屏幕比例</strong>：16:9</p></li><li><p><strong>屏幕分辨率</strong>：1920×1080</p></li><li><p><strong>屏幕类型</strong>：FHD背光</p></li><li><p><strong>触摸功能</strong>：不支持</p></li><li><p><em class=\"icon i-pc-screen\"></em></p><p>显示屏</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p></li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>颜色</strong>：红黑</p></li><li><p><strong>厚度</strong>：29.2mm</p></li><li><p><strong>机身尺寸</strong>：380*251*29.2mm</p></li><li><p><strong>重量</strong>：2.45</p></li><li><p><em class=\"icon i-pc-appearance\"></em></p><p>外观</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p></li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>电池类型</strong>：4芯锂电池</p></li><li><p><strong>续航时间</strong>：视使用环境而定</p></li><li><p><strong>电源适配器</strong>：输出：直流19 伏, 6.32 安, 120 瓦，输入：交流100 -240 伏，50/60 Hz</p></li><li><p><em class=\"icon i-pc-battery\"></em></p><p>电池</p></li><li><p><br/></p></li><em class=\"ng-iconfont p-down-arrow\"></em><li><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p></li></ul><li><p><br/></p></li><ul class=\" list-paddingleft-2\" style=\"list-style-type: square;\"><li><p><strong>操作系统</strong>：Windows10</p></li><li><p><strong>蓝牙功能</strong>：蓝牙4.0</p></li><li><p><strong>局域网</strong>：10/100/1000Mbps</p></li><li><p><strong>无线局域网</strong>：802.11b/g/n</p></li><li><p><em class=\"icon i-others\"></em></p><p>其他</p></li><li><p><em class=\"ng-iconfont p-down-arrow\" style=\"font-family: sans-serif; font-size: 16px;\"><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/cf3c2144e601446eac1ef83a0f199b33.jpg\" class=\"err-product\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/d6b542d712194956bd2bf256a5d07b20.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/5314cefe565c40a8bc00df060c77e4d7.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/94ad58b4060e42848fabe29b9d4f7678.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/c82a30d4970e4abca1e2ed60dfd52154.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/c7901716a24e4ef697abef181bc1f8c1.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/afce2c3909b24dd58bb98ec0f9cc6d39.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/cee1c9b978044b2981d6e6a6c3d88bfb.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/657acbaf0541463cad2c09c8fd2cbc6d.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/23d83f79f0a940a28e4fbdb09ae23f7a.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/><img alt=\"\" src=\"/Storage/Shop/1/Products/741/remark/7ab4f35206b24b3d8221fd0692ea6fff.jpg\" class=\"err-product\" height=\"421\" width=\"750\" style=\"font-family: &quot;microsoft yahei&quot;; font-size: 12px;\"/></em><br/></p></li><li><p><em class=\"ng-iconfont p-down-arrow\" style=\"font-family: sans-serif; font-size: 16px;\"><br/></em></p></li></ul></ul><p><br/></p>'),(0,742,NULL,'<p><img src=\"/Storage/Shop/1/Products/742/remark/066f89de309e43d6bea2ecfcd2b8443c.jpg\" style=\"\" title=\"101253536820931769317251_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/742/remark/5c562cfea87c490cba1a434502100c25.jpg\" style=\"\" title=\"190559784352025193155020_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/742/remark/08bf923be07e4c7c93e084a3822da754.jpg\" style=\"\" title=\"203256408834708158545700_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/742/remark/66db7d29bf434322a4b73055a70fa5db.jpg\" style=\"\" title=\"171061270229616959709800_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/742/remark/d57ac29ca1754076a3dc2d7c29032485.jpg\" style=\"\" title=\"105206931911539331857653_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/742/remark/7570f7b501ea4905bef8fdf3e8c729ee.jpg\" style=\"\" title=\"163129632489029728231700_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/742/remark/fd3f51582a02420c9cbd1d43f912b9c0.jpg\" style=\"\" title=\"101253536820931769317251_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/742/remark/1e60fea2ac28438f8dc8086294c7ff46.jpg\" style=\"\" title=\"190559784352025193155020_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/742/remark/47a2e5f7617b46e385bf8fab8d7d9d4d.jpg\" style=\"\" title=\"203256408834708158545700_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/742/remark/8da9a7be492c4b74a8ece6473decf9d8.jpg\" style=\"\" title=\"171061270229616959709800_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/742/remark/9eafe185844347efac120942262cec51.jpg\" style=\"\" title=\"105206931911539331857653_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/742/remark/88f58ac252a743cda21c28fb1ac8527a.jpg\" style=\"\" title=\"163129632489029728231700_x.jpg\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/742/remark/afb0f8b1bdd14f3ea202f4d1c082402e.jpg\" title=\"190559784352025193155020_x.jpg\" alt=\"190559784352025193155020_x.jpg\" width=\"575\" height=\"585\" style=\"width: 575px; height: 585px;\"/></p>'),(0,743,NULL,'<p><img src=\"/Storage/Shop/1/Products/743/remark/7dd6211c37fb441ea49338d504d2ab93.jpg\" title=\"164832297599576932834000_x.jpg\" alt=\"164832297599576932834000_x.jpg\" width=\"573\" height=\"1520\" style=\"width: 573px; height: 1520px;\"/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/743/remark/6148756bdca94563a75e8398864cfeac.jpg\" title=\"164832297599576932834000_x.jpg\" alt=\"164832297599576932834000_x.jpg\" width=\"620\" height=\"1520\" style=\"width: 620px; height: 1520px;\"/></p>'),(0,744,NULL,'<p><img src=\"/Storage/Shop/1/Products/744/remark/a8fcbaf144bb417eba26c9a5ad07c44a.jpg\" title=\"102294239716504473127328_x.jpg\" alt=\"102294239716504473127328_x.jpg\" width=\"552\" height=\"2865\" style=\"width: 552px; height: 2865px;\"/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/744/remark/adb6952c035d4a7ea9892cba07e8f24c.jpg\" title=\"102294239716504473127328_x.jpg\" alt=\"102294239716504473127328_x.jpg\" width=\"588\" height=\"3130\" style=\"width: 588px; height: 3130px;\"/></p>'),(0,745,NULL,'<p><img src=\"/Storage/Shop/1/Products/745/remark/77a083526b0443269405c7a8cc746252.jpg\" title=\"706815300143606758614590_x.jpg\" alt=\"706815300143606758614590_x.jpg\" width=\"606\" height=\"531\" style=\"width: 606px; height: 531px;\"/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/745/remark/4197cf6a0a584f69baddad05432f30f2.jpg\" title=\"706815300143606758614590_x.jpg\" alt=\"706815300143606758614590_x.jpg\" width=\"618\" height=\"531\" style=\"width: 618px; height: 531px;\"/></p>'),(0,746,NULL,'<p><img src=\"/Storage/Shop/1/Products/746/remark/a1a4b92bf05d42c2bcbb0361093de4af.jpg\" style=\"\" title=\"254092320207544370723070_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/746/remark/025e869ed1be4d2293512e304fc3f33d.jpg\" style=\"\" title=\"180623610314940567046598_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/746/remark/f807022811a14e888ea4c0f146088b73.jpg\" style=\"\" title=\"158956928575064312669470_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/746/remark/c83697ad22734a929676b0e4516e1928.jpg\" style=\"\" title=\"109830556713545618383822_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/746/remark/e39ab74412464d849cefd67b1d110329.jpg\" style=\"\" title=\"154068696448416553795900_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/746/remark/e1c154e60f6c4209a11cf90af0cd09d8.jpg\" style=\"\" title=\"164945020811100395099595_x.jpg\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/746/remark/3e44fe7dfb8747ce8f325b66bf9c45e9.jpg\" style=\"\" title=\"254092320207544370723070_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/746/remark/585fbf4a63484ffeb8b10f812fba9c83.jpg\" style=\"\" title=\"180623610314940567046598_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/746/remark/e0e1111dd91c4af5b19e0d79cf383595.jpg\" style=\"\" title=\"109830556713545618383822_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/746/remark/f3765e2942c146d480eff315debce4b8.jpg\" style=\"\" title=\"115119514115970317649003_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/746/remark/bb9a0648241b4e47bc5076c8772c2b7c.jpg\" style=\"\" title=\"154068696448416553795900_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/746/remark/0f4b6010a24e4592b24d9ddea0bfe3b8.jpg\" style=\"\" title=\"164945020811100395099595_x.jpg\"/></p><p><br/></p>'),(0,747,NULL,'<p><img src=\"/Storage/Shop/1/Products/747/remark/e9efd1dd5bd34ebebb5282b693e4c6af.jpg\" style=\"\" title=\"200199594919382729362106_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/747/remark/88c792f8cf3c4ef295a8f9d084638635.jpg\" style=\"\" title=\"254513520974574754360700_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/747/remark/262ca9e340dc447f8219660f1a00342e.jpg\" style=\"\" title=\"205682348789378083682400_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/747/remark/5590b7e0207d4bbe97e1490c5ae50d6c.jpg\" style=\"\" title=\"114571995611894190766661_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/747/remark/47b9f8ecd3cf4c9bb864fd9fc4f3655e.jpg\" style=\"\" title=\"191263094576494936488280_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/747/remark/3529c330fdc945afaf1cc0b317ea8e43.jpg\" style=\"\" title=\"188828379140541586029510_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/747/remark/7b041dc4206b422d9f3e8a5f72867ce6.jpg\" style=\"\" title=\"161380848415949423087457_x.jpg\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/747/remark/1b6c6500b6fe43fcbffdd38f4fd8f7e9.jpg\" style=\"\" title=\"200199594919382729362106_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/747/remark/3a9cdf6c06124a9eb118ad6504577cd0.jpg\" style=\"\" title=\"254513520974574754360700_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/747/remark/cb29387da9114ae89dfb15ef6ab42703.jpg\" style=\"\" title=\"205682348789378083682400_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/747/remark/4783676efdb342899d06deb4e4763e74.jpg\" style=\"\" title=\"191263094576494936488280_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/747/remark/668c044c30644196bff4fb74ccecc74e.jpg\" style=\"\" title=\"188828379140541586029510_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/747/remark/69292f4c148c40f88f68eff5efd836bc.jpg\" style=\"\" title=\"161380848415949423087457_x.jpg\"/></p><p><br/></p>'),(0,748,NULL,'<p><img src=\"/Storage/Shop/1/Products/748/remark/d299566eac8f4711b4bc8eb304f578d0.jpg\" style=\"\" title=\"114356009114902161955534_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/748/remark/5b89734ffcd7420693690ca9c62ac6e5.jpg\" style=\"\" title=\"158966296352170822019910_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/748/remark/a41d43b300ac429d9ebc24042a8eb274.jpg\" style=\"\" title=\"180369332776254090383480_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/748/remark/9c6f543a57a94a39bd330e48dd1be45b.jpg\" style=\"\" title=\"188754394139208134727600_x.jpg\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/748/remark/3d007f9ba8e749d0bc83b3c98b2416ac.jpg\" style=\"\" title=\"114356009114902161955534_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/748/remark/86f1a6d6082b434db6ad4bb3282296f5.jpg\" style=\"\" title=\"158966296352170822019910_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/748/remark/1136e71509234753b4282bae4b635b42.jpg\" style=\"\" title=\"180369332776254090383480_x (1).jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/748/remark/94858ab1de0549f6820ecf3eb6a3984a.jpg\" style=\"\" title=\"188754394139208134727600_x.jpg\"/></p><p><br/></p>'),(0,749,NULL,'<p><img src=\"/Storage/Shop/1/Products/749/remark/4445b2b71c1e42a3b246048b457fab30.jpg\" style=\"\" title=\"130770327070953959079750_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/749/remark/971682811e30463280cfab41cfe4c842.jpg\" style=\"\" title=\"124550073013133335991177_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/749/remark/277c1bd230d04d4db1db6538cb1d0059.jpg\" style=\"\" title=\"203029478111080927671545_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/749/remark/92310a3c88324edf84cf0b23f8731be0.jpg\" style=\"\" title=\"879915146185111203465000_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/749/remark/c791a0a1a89043ae8d8fd779a3a17edc.jpg\" style=\"\" title=\"172422154169193566069950_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/749/remark/5d242f4b062a43c5ab02093d682010d6.jpg\" style=\"\" title=\"200493045321581547287850_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/749/remark/427f72853e3f461ab609321da16b09b0.jpg\" style=\"\" title=\"336393284157900838633750_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/749/remark/2e44185798c44de8b4d91eab0cd7eeae.jpg\" style=\"\" title=\"396771162199382333953840_x.jpg\"/></p><p><br/></p>',0,0,NULL,NULL,NULL,'<p><img src=\"/Storage/Shop/1/Products/749/remark/fb0e42094f5a4b14a1d58be6d3240659.jpg\" style=\"\" title=\"130770327070953959079750_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/749/remark/5a44fe62fa82472ebb28ae4090715308.jpg\" style=\"\" title=\"203029478111080927671545_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/749/remark/075b106f3e804eedbb23afd3857021dd.jpg\" style=\"\" title=\"200493045321581547287850_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/749/remark/ca67ad162b3c480582b078597ae9af62.jpg\" style=\"\" title=\"172422154169193566069950_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/749/remark/487724d0148b4c07afdad233edf7c985.jpg\" style=\"\" title=\"336393284157900838633750_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/749/remark/05a253b787dd4814b30ced2a1747ae89.jpg\" style=\"\" title=\"879915146185111203465000_x.jpg\"/></p><p><img src=\"/Storage/Shop/1/Products/749/remark/4ca0d2d1cc8141e396e1bc2b0de38390.jpg\" style=\"\" title=\"396771162199382333953840_x.jpg\"/></p><p><br/></p>');
/*!40000 ALTER TABLE `mall_productdescription` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_productdescriptiontemplate`
--

DROP TABLE IF EXISTS `mall_productdescriptiontemplate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_productdescriptiontemplate` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `Name` varchar(100) NOT NULL COMMENT '板式名称',
  `Position` int(11) NOT NULL COMMENT '位置（上、下）',
  `Content` varchar(4000) NOT NULL COMMENT 'PC端版式',
  `MobileContent` varchar(4000) DEFAULT NULL COMMENT '移动端版式',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_productdescriptiontemplate`
--

LOCK TABLES `mall_productdescriptiontemplate` WRITE;
/*!40000 ALTER TABLE `mall_productdescriptiontemplate` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_productdescriptiontemplate` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_productladderprice`
--

DROP TABLE IF EXISTS `mall_productladderprice`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_productladderprice` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '阶梯价格ID',
  `ProductId` bigint(20) NOT NULL COMMENT '商品ID',
  `MinBath` int(11) NOT NULL COMMENT '最小批量',
  `MaxBath` int(11) NOT NULL COMMENT '最大批量',
  `Price` decimal(18,2) NOT NULL COMMENT '价格',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=179 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_productladderprice`
--

LOCK TABLES `mall_productladderprice` WRITE;
/*!40000 ALTER TABLE `mall_productladderprice` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_productladderprice` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_productrelationproduct`
--

DROP TABLE IF EXISTS `mall_productrelationproduct`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_productrelationproduct` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ProductId` bigint(20) NOT NULL COMMENT '商品id',
  `Relation` varchar(255) NOT NULL COMMENT '推荐的商品id列表，以‘，’分隔',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='推荐商品';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_productrelationproduct`
--

LOCK TABLES `mall_productrelationproduct` WRITE;
/*!40000 ALTER TABLE `mall_productrelationproduct` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_productrelationproduct` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_productshopcategory`
--

DROP TABLE IF EXISTS `mall_productshopcategory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_productshopcategory` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ProductId` bigint(20) NOT NULL,
  `ShopCategoryId` bigint(20) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Product_ProductShopCategory` (`ProductId`) USING BTREE,
  KEY `FK_ShopCategory_ProductShopCategory` (`ShopCategoryId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2821 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_productshopcategory`
--

LOCK TABLES `mall_productshopcategory` WRITE;
/*!40000 ALTER TABLE `mall_productshopcategory` DISABLE KEYS */;
INSERT INTO `mall_productshopcategory` VALUES (1851,709,351),(1853,707,351),(1854,703,352),(1855,701,354),(1857,700,353),(1858,712,351),(1860,718,360),(1861,717,360),(1862,716,356),(1863,715,358),(1864,714,358),(1865,713,351),(1866,708,351),(1867,722,360),(1868,723,360),(1870,724,351),(1871,725,351),(1872,726,351),(1874,738,360),(1875,741,358),(1876,736,360);
/*!40000 ALTER TABLE `mall_productshopcategory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_productvisti`
--

DROP TABLE IF EXISTS `mall_productvisti`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_productvisti` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `ProductId` bigint(20) NOT NULL,
  `Date` datetime NOT NULL,
  `VistiCounts` bigint(20) NOT NULL COMMENT '浏览次数',
  `VisitUserCounts` bigint(20) NOT NULL COMMENT '浏览人数',
  `PayUserCounts` bigint(20) NOT NULL COMMENT '付款人数',
  `SaleCounts` bigint(20) NOT NULL COMMENT '商品销售数量',
  `SaleAmounts` decimal(18,2) NOT NULL COMMENT '商品销售额',
  `OrderCounts` bigint(20) NOT NULL DEFAULT '0' COMMENT '订单总数',
  `StatisticFlag` bit(1) NOT NULL COMMENT '是否已经统计(0：未统计,1已统计)',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Product_ProductVisti` (`ProductId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=8510 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_productvisti`
--

LOCK TABLES `mall_productvisti` WRITE;
/*!40000 ALTER TABLE `mall_productvisti` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_productvisti` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_receivingaddressconfig`
--

DROP TABLE IF EXISTS `mall_receivingaddressconfig`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_receivingaddressconfig` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `AddressId_City` text,
  `AddressId` text NOT NULL COMMENT '逗号分隔',
  `ShopId` bigint(20) NOT NULL COMMENT '预留字段，防止将来其他商家一并支持货到付款',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_RACShopId` (`ShopId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_receivingaddressconfig`
--

LOCK TABLES `mall_receivingaddressconfig` WRITE;
/*!40000 ALTER TABLE `mall_receivingaddressconfig` DISABLE KEYS */;
INSERT INTO `mall_receivingaddressconfig` VALUES (1,'\'2\',\'222\',\'233\',\'245\',\'251\',\'468\',\'482\',\'493\',\'501\',\'1034\',\'1042\',\'1050\',\'1696\',\'1710\',\'1717\',\'1726\',\'1740\',\'1788\',\'1813\',\'1823\',\'1839\',\'1852\',\'1865\',\'1897\',\'1909\',\'2052\',\'2208\',\'2234\',\'2243\',\'2251\',\'2311\',\'2398\',\'2534\',\'2594\',\'2749\',\'2841\',\'2959\',\'3012\',\'3041\',\'3100\',\'3130\',\'3166\',\'3222\',\'3422\',\'3601\',\'3634\',\'3651\'','\'3\',\'4\',\'5\',\'6\',\'7\',\'8\',\'9\',\'10\',\'11\',\'12\',\'13\',\'14\',\'15\',\'16\',\'17\',\'18\',\'223\',\'224\',\'225\',\'226\',\'227\',\'228\',\'229\',\'230\',\'231\',\'232\',\'234\',\'235\',\'236\',\'237\',\'238\',\'239\',\'240\',\'241\',\'242\',\'243\',\'244\',\'246\',\'247\',\'248\',\'249\',\'250\',\'252\',\'253\',\'254\',\'255\',\'256\',\'257\',\'258\',\'259\',\'260\',\'261\',\'262\',\'263\',\'264\',\'469\',\'470\',\'471\',\'472\',\'473\',\'474\',\'475\',\'476\',\'477\',\'478\',\'479\',\'480\',\'481\',\'483\',\'484\',\'485\',\'486\',\'487\',\'488\',\'489\',\'490\',\'491\',\'492\',\'494\',\'495\',\'496\',\'497\',\'498\',\'499\',\'500\',\'502\',\'503\',\'504\',\'505\',\'506\',\'507\',\'508\',\'1035\',\'1036\',\'1037\',\'1038\',\'1039\',\'1040\',\'1041\',\'1124\',\'1125\',\'1043\',\'1044\',\'1045\',\'1046\',\'1047\',\'1048\',\'1049\',\'1126\',\'1051\',\'1052\',\'1053\',\'1054\',\'1055\',\'1056\',\'1057\',\'1697\',\'1698\',\'1699\',\'1700\',\'1701\',\'1702\',\'1703\',\'1704\',\'1705\',\'1706\',\'1707\',\'1708\',\'1709\',\'1711\',\'1712\',\'1713\',\'1714\',\'1715\',\'1716\',\'1718\',\'1719\',\'1720\',\'1721\',\'1722\',\'1723\',\'1724\',\'1725\',\'1727\',\'1728\',\'1729\',\'1730\',\'1731\',\'1732\',\'1733\',\'1734\',\'1735\',\'1736\',\'1737\',\'1738\',\'1739\',\'1741\',\'1742\',\'1743\',\'1744\',\'1745\',\'1746\',\'1747\',\'1748\',\'1749\',\'1789\',\'1790\',\'1791\',\'1792\',\'1793\',\'1794\',\'1814\',\'1815\',\'1816\',\'1817\',\'1818\',\'1819\',\'1820\',\'1821\',\'1822\',\'1824\',\'1825\',\'1826\',\'1827\',\'1828\',\'1829\',\'1830\',\'1831\',\'1832\',\'1834\',\'1835\',\'1840\',\'1841\',\'1842\',\'1843\',\'1844\',\'1845\',\'1846\',\'1847\',\'1848\',\'1849\',\'1850\',\'1851\',\'1853\',\'1854\',\'1855\',\'1856\',\'1857\',\'1858\',\'1859\',\'1860\',\'1861\',\'1862\',\'1863\',\'1864\',\'1866\',\'1867\',\'1868\',\'1869\',\'1870\',\'1871\',\'1872\',\'1873\',\'1874\',\'1898\',\'1899\',\'1900\',\'1901\',\'1902\',\'1903\',\'1904\',\'1905\',\'1906\',\'1907\',\'1908\',\'1910\',\'1911\',\'1912\',\'1913\',\'1914\',\'1915\',\'1916\',\'1917\',\'1918\',\'1919\',\'1920\',\'2053\',\'2054\',\'2055\',\'2056\',\'2209\',\'2210\',\'2211\',\'2212\',\'2213\',\'2214\',\'32627\',\'32628\',\'32629\',\'32630\',\'32631\',\'32632\',\'32633\',\'32634\',\'32760\',\'32761\',\'32762\',\'32763\',\'32764\',\'32765\',\'32766\',\'32767\',\'32768\',\'32769\',\'32770\',\'32771\',\'2252\',\'2253\',\'2254\',\'2255\',\'2256\',\'2257\',\'2258\',\'2259\',\'2260\',\'2261\',\'2262\',\'2263\',\'2264\',\'2265\',\'2266\',\'2267\',\'2268\',\'2269\',\'2270\',\'2271\',\'2272\',\'2273\',\'2274\',\'2275\',\'2276\',\'2277\',\'2278\',\'2279\',\'2280\',\'2281\',\'2282\',\'2283\',\'2284\',\'2285\',\'2286\',\'2287\',\'2288\',\'2289\',\'2312\',\'2313\',\'2314\',\'2315\',\'2316\',\'2317\',\'2399\',\'2400\',\'2401\',\'2402\',\'2403\',\'2404\',\'2405\',\'2406\',\'2407\',\'2408\',\'2535\',\'2536\',\'2537\',\'2538\',\'2539\',\'2540\',\'2541\',\'2542\',\'2543\',\'2544\',\'2595\',\'2596\',\'2597\',\'2598\',\'2599\',\'2600\',\'2601\',\'2602\',\'2603\',\'2604\',\'2605\',\'2606\',\'2607\',\'2608\',\'2750\',\'2751\',\'2752\',\'2753\',\'2754\',\'2755\',\'2756\',\'2757\',\'2758\',\'2759\',\'2760\',\'2842\',\'2843\',\'2844\',\'2845\',\'2846\',\'2847\',\'2848\',\'2849\',\'2850\',\'2851\',\'2852\',\'2853\',\'2960\',\'2961\',\'2962\',\'2963\',\'2964\',\'2965\',\'2966\',\'3013\',\'3014\',\'3015\',\'3016\',\'3017\',\'3018\',\'3019\',\'3020\',\'3021\',\'3042\',\'3043\',\'3044\',\'3045\',\'3046\',\'3047\',\'3048\',\'3101\',\'3102\',\'3103\',\'3131\',\'3132\',\'3133\',\'3134\',\'3167\',\'3168\',\'3169\',\'3170\',\'3171\',\'3172\',\'3173\',\'3174\',\'3175\',\'3223\',\'3224\',\'3225\',\'3226\',\'3227\',\'3228\',\'3229\',\'3423\',\'3424\',\'3425\',\'3426\',\'3427\',\'3428\',\'3429\',\'3430\',\'3431\',\'3432\',\'3433\',\'3434\',\'3435\',\'3602\',\'3603\',\'3604\',\'3605\',\'3606\',\'3607\',\'3608\',\'3609\',\'3610\',\'3611\',\'3612\',\'3613\',\'3614\',\'3615\',\'3635\',\'3636\',\'3637\',\'3638\',\'3639\'',1);
/*!40000 ALTER TABLE `mall_receivingaddressconfig` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_rechargepresentrule`
--

DROP TABLE IF EXISTS `mall_rechargepresentrule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_rechargepresentrule` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ChargeAmount` decimal(18,2) NOT NULL COMMENT '充多少',
  `PresentAmount` decimal(18,2) NOT NULL COMMENT '送多少',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=MyISAM DEFAULT CHARSET=utf8 COMMENT='充值赠送规则';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_rechargepresentrule`
--

LOCK TABLES `mall_rechargepresentrule` WRITE;
/*!40000 ALTER TABLE `mall_rechargepresentrule` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_rechargepresentrule` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_refundreason`
--

DROP TABLE IF EXISTS `mall_refundreason`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_refundreason` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AfterSalesText` varchar(100) DEFAULT NULL COMMENT '售后原因',
  `Sequence` int(11) NOT NULL DEFAULT '100' COMMENT '排序',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=34 DEFAULT CHARSET=utf8 COMMENT='售后原因';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_refundreason`
--

LOCK TABLES `mall_refundreason` WRITE;
/*!40000 ALTER TABLE `mall_refundreason` DISABLE KEYS */;
INSERT INTO `mall_refundreason` VALUES (28,'不想买了',100),(29,'其他',100),(30,'质量有问题',100);
/*!40000 ALTER TABLE `mall_refundreason` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_role`
--

DROP TABLE IF EXISTS `mall_role`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_role` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `RoleName` varchar(100) NOT NULL COMMENT '角色名称',
  `Description` varchar(1000) NOT NULL COMMENT '说明',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=50 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_role`
--

LOCK TABLES `mall_role` WRITE;
/*!40000 ALTER TABLE `mall_role` DISABLE KEYS */;
INSERT INTO `mall_role` VALUES (46,0,'客户体验','客户体验'),(47,0,'演示组','演示组'),(48,0,'管理员','管理员'),(49,1,'体验组','体验组');
/*!40000 ALTER TABLE `mall_role` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_roleprivilege`
--

DROP TABLE IF EXISTS `mall_roleprivilege`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_roleprivilege` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Privilege` int(11) NOT NULL COMMENT '权限ID',
  `RoleId` bigint(20) NOT NULL COMMENT '角色ID',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Role_RolePrivilege` (`RoleId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=1816 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_roleprivilege`
--

LOCK TABLES `mall_roleprivilege` WRITE;
/*!40000 ALTER TABLE `mall_roleprivilege` DISABLE KEYS */;
INSERT INTO `mall_roleprivilege` VALUES (1662,2001,47),(1663,2002,47),(1664,2003,47),(1665,2004,47),(1666,2006,47),(1667,2007,47),(1668,3001,47),(1669,3002,47),(1670,3009,47),(1671,3003,47),(1672,3004,47),(1673,3005,47),(1674,3006,47),(1675,3007,47),(1676,3008,47),(1677,8004,47),(1678,8005,47),(1679,8006,47),(1680,8007,47),(1682,2001,48),(1683,2002,48),(1684,2003,48),(1685,2004,48),(1686,2006,48),(1687,2007,48),(1688,3001,48),(1689,3002,48),(1690,3009,48),(1691,3003,48),(1692,3004,48),(1693,3005,48),(1694,3006,48),(1695,3007,48),(1696,3008,48),(1697,4001,48),(1698,4002,48),(1699,4009,48),(1700,4010,48),(1701,4003,48),(1702,4004,48),(1703,4005,48),(1704,4006,48),(1705,4007,48),(1706,4008,48),(1707,5001,48),(1708,5002,48),(1709,5003,48),(1710,5004,48),(1711,5005,48),(1712,6002,48),(1713,6003,48),(1714,6004,48),(1715,6005,48),(1716,7002,48),(1717,7003,48),(1718,7004,48),(1719,7101,48),(1720,7102,48),(1721,7103,48),(1722,7104,48),(1723,8001,48),(1724,8002,48),(1725,8003,48),(1726,8004,48),(1727,8005,48),(1728,8006,48),(1729,8007,48),(1730,9020,48),(1731,9001,48),(1732,9002,48),(1733,9013,48),(1734,9003,48),(1735,9004,48),(1736,9005,48),(1737,9006,48),(1738,9007,48),(1739,9008,48),(1740,9011,48),(1741,9012,48),(1742,9014,48),(1743,9009,48),(1744,9010,48),(1745,10001,48),(1746,10002,48),(1747,10005,48),(1748,12001,48),(1749,12002,48),(1750,2001,46),(1751,2002,46),(1752,2003,46),(1753,2004,46),(1754,2006,46),(1755,2007,46),(1756,3001,46),(1757,3002,46),(1758,3009,46),(1759,3003,46),(1760,3004,46),(1761,3005,46),(1762,3006,46),(1763,3007,46),(1764,3008,46),(1765,4001,46),(1766,4002,46),(1767,4009,46),(1768,4010,46),(1769,4003,46),(1770,4004,46),(1771,4005,46),(1772,4006,46),(1773,4007,46),(1774,4008,46),(1775,5001,46),(1776,5002,46),(1777,5003,46),(1778,5004,46),(1779,5005,46),(1780,6002,46),(1781,6003,46),(1782,6004,46),(1783,6005,46),(1784,7002,46),(1785,7003,46),(1786,7004,46),(1787,7101,46),(1788,7102,46),(1789,7103,46),(1790,7104,46),(1791,8004,46),(1792,8005,46),(1793,8006,46),(1794,8007,46),(1795,9020,46),(1796,9001,46),(1797,9002,46),(1798,9013,46),(1799,9003,46),(1800,9004,46),(1801,9005,46),(1802,9006,46),(1803,9007,46),(1804,9008,46),(1805,9011,46),(1806,9012,46),(1807,9014,46),(1808,9009,46),(1809,9010,46),(1810,10001,46),(1811,10002,46),(1812,10005,46),(1813,12001,46),(1814,12002,46),(1815,4008,49);
/*!40000 ALTER TABLE `mall_roleprivilege` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_searchproduct`
--

DROP TABLE IF EXISTS `mall_searchproduct`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_searchproduct` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ProductId` bigint(20) NOT NULL COMMENT '商品Id',
  `ProductName` varchar(100) NOT NULL DEFAULT '' COMMENT '商品名称',
  `ShopId` bigint(20) NOT NULL DEFAULT '0' COMMENT '店铺Id',
  `ShopName` varchar(100) DEFAULT '' COMMENT '店铺名称',
  `BrandId` bigint(20) NOT NULL DEFAULT '0' COMMENT '品牌Id',
  `BrandName` varchar(100) DEFAULT '' COMMENT '品牌名称',
  `BrandLogo` varchar(1000) DEFAULT '' COMMENT '品牌Logo',
  `FirstCateId` bigint(20) NOT NULL DEFAULT '0' COMMENT '一级分类Id',
  `FirstCateName` varchar(100) NOT NULL DEFAULT '' COMMENT '一级分类名称',
  `SecondCateId` bigint(20) NOT NULL COMMENT '二级分类Id',
  `SecondCateName` varchar(100) NOT NULL DEFAULT '' COMMENT '二级分类名称',
  `ThirdCateId` bigint(20) NOT NULL COMMENT '三级分类Id',
  `ThirdCateName` varchar(100) NOT NULL DEFAULT '' COMMENT '三级分类名称',
  `AttrValues` text COMMENT '属性值Id用英文逗号分隔',
  `Comments` int(11) NOT NULL DEFAULT '0' COMMENT '评论数',
  `SaleCount` int(11) NOT NULL DEFAULT '0' COMMENT '成交量',
  `SalePrice` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '售价',
  `OnSaleTime` datetime NOT NULL COMMENT '上架时间',
  `ImagePath` varchar(100) NOT NULL DEFAULT '' COMMENT '商品图片地址',
  `CanSearch` bit(1) NOT NULL DEFAULT b'0' COMMENT '可以搜索',
  PRIMARY KEY (`Id`) USING BTREE,
  UNIQUE KEY `IX_ProductId` (`ProductId`) USING BTREE,
  KEY `IX_ShopId` (`ShopId`) USING BTREE,
  KEY `IX_BrandId` (`BrandId`) USING BTREE,
  KEY `IX_FirstCateId` (`FirstCateId`) USING BTREE,
  KEY `IX_SecondCateId` (`SecondCateId`) USING BTREE,
  KEY `IX_ThirdCateId` (`ThirdCateId`) USING BTREE,
  KEY `IX_Comments` (`Comments`) USING BTREE,
  KEY `IX_SaleCount` (`SaleCount`) USING BTREE,
  KEY `IX_OnSaleTime` (`OnSaleTime`) USING BTREE,
  KEY `IX_CanSearch` (`CanSearch`) USING BTREE,
  KEY `IX_SalePrice` (`SalePrice`) USING BTREE,
  FULLTEXT KEY `ProductName` (`ProductName`)
) ENGINE=InnoDB AUTO_INCREMENT=1861 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_searchproduct`
--

LOCK TABLES `mall_searchproduct` WRITE;
/*!40000 ALTER TABLE `mall_searchproduct` DISABLE KEYS */;
INSERT INTO `mall_searchproduct` VALUES (1798,699,'三只松鼠_开口松子218gx2袋零食坚果特产炒货东北红松子原味',1,'官方自营店',319,'三只松鼠','/Storage/Plat/Brand/logo_319.jpg',1,'食品、酒类、特产',2,'坚果',3,'松子','811',0,0,49.90,'2017-02-13 17:32:45','/Storage/Shop/1/Products/699',''),(1799,700,'三只松鼠_坚果组合613g零食坚果特产碧根果夏威夷果开口松子原味',1,'官方自营店',319,'三只松鼠','/Storage/Plat/Brand/logo_319.jpg',1,'食品、酒类、特产',2,'坚果',7,'碧根果','812',0,0,59.90,'2017-02-13 17:36:10','/Storage/Shop/1/Products/700',''),(1800,701,'三只松鼠 坚果炒货 休闲零食 纸皮核桃210g/袋',1,'官方自营店',319,'三只松鼠','/Storage/Plat/Brand/logo_319.jpg',1,'食品、酒类、特产',2,'坚果',6,'核桃','810',0,0,29.90,'2017-02-13 17:39:24','/Storage/Shop/1/Products/701',''),(1801,702,'卫龙 休闲零食 辣条 小面筋 办公室休闲食品 22g*20包(新老包装随机发货)',1,'官方自营店',320,'卫龙','/Storage/Plat/Brand/logo_320.jpg',1,'食品、酒类、特产',4,'辣条',5,'卫龙','813',0,0,11.90,'2017-02-13 17:42:06','/Storage/Shop/1/Products/702',''),(1802,703,'卫龙 休闲零食 亲嘴条 辣条 400g/袋',1,'官方自营店',320,'卫龙','/Storage/Plat/Brand/logo_320.jpg',1,'食品、酒类、特产',4,'辣条',5,'卫龙','814',0,0,10.80,'2017-02-13 17:48:53','/Storage/Shop/1/Products/703','\0'),(1803,704,'ONLY2017早春新品宽松猫咪图案假两件针织衫女L|117124507 G43花灰 170/88A/L',1,'官方自营店',321,'only','/Storage/Plat/Brand/logo_321.jpg',8,'男装、女装、童装',9,'女装',13,'针织衫','816',0,0,499.00,'2017-02-13 17:53:09','/Storage/Shop/1/Products/704',''),(1804,705,'ONLY2017早春新品大V领宽松下摆开衩长袖针织衫女ES|117124502 F17庆典红 165/84A/M',1,'官方自营店',321,'only','/Storage/Plat/Brand/logo_321.jpg',8,'男装、女装、童装',9,'女装',10,'外套','819',0,0,399.00,'2017-02-13 17:55:40','/Storage/Shop/1/Products/705',''),(1805,706,'ONLY2016早春新品拼色宽松太空棉卫衣女E|11639R511 07B酒红色 165/84A/M',1,'官方自营店',321,'only','/Storage/Plat/Brand/logo_321.jpg',8,'男装、女装、童装',9,'女装',10,'外套','819',0,0,234.00,'2017-02-13 17:58:44','/Storage/Shop/1/Products/706',''),(1806,707,'ONLY春季新品含莱卡面料五分袖修身露肩一字领针织连衣裙女E|116361504 010黑 175/92A/XL',1,'官方自营店',321,'only','/Storage/Plat/Brand/logo_321.jpg',8,'男装、女装、童装',9,'女装',11,'裙子',NULL,0,0,175.00,'2017-02-13 18:01:30','/Storage/Shop/1/Products/707',''),(1807,708,'ONLY春季新品纯棉修身高腰镂空蕾丝连衣裙女L|116307501 021奶白 155/76A/XS',1,'官方自营店',321,'only','/Storage/Plat/Brand/logo_321.jpg',8,'男装、女装、童装',9,'女装',11,'裙子','817',0,0,450.00,'2017-02-13 18:03:48','/Storage/Shop/1/Products/708',''),(1808,709,'ONLY2016春季新品T恤衬衫叠搭纯棉两件套连衣裙女T|116360504 10D炭花灰 165/84A/M',1,'官方自营店',321,'only','/Storage/Plat/Brand/logo_321.jpg',8,'男装、女装、童装',9,'女装',12,'衬衣','816',0,0,599.00,'2017-02-13 18:06:19','/Storage/Shop/1/Products/709','\0'),(1809,710,'ONLY2016早春新品纯棉宽松徽章贴布牛仔背带裙女T|116342524 390洗水牛仔蓝(928) 155/76A/XS',1,'官方自营店',321,'only','/Storage/Plat/Brand/logo_321.jpg',8,'男装、女装、童装',9,'女装',11,'裙子','817',0,0,250.00,'2017-02-13 18:11:32','/Storage/Shop/1/Products/710','\0'),(1810,711,'FOREVER21 甜美花朵抽褶吊带连衣裙 黑色/铁锈色 S',1,'官方自营店',0,NULL,NULL,8,'男装、女装、童装',9,'女装',11,'裙子','817',0,0,169.00,'2017-02-14 09:31:14','/Storage/Shop/1/Products/711','\0'),(1811,712,'[Forever21 Contemporary]纯色优雅及膝短袖连衣裙 香草色 S',1,'官方自营店',0,NULL,NULL,8,'男装、女装、童装',9,'女装',11,'裙子','817',0,0,229.00,'2017-02-14 09:35:30','/Storage/Shop/1/Products/712','\0'),(1812,713,'XX FOREVER21 短款撞色条纹无袖针织衫 红色/白色 M',1,'官方自营店',0,NULL,NULL,8,'男装、女装、童装',9,'女装',13,'针织衫',NULL,0,0,50.00,'2017-02-14 09:39:07','/Storage/Shop/1/Products/713','\0'),(1813,714,'Apple MacBook Air 13.3英寸笔记本电脑 银色(Core i5 处理器/8GB内存/128GB SSD闪存 MMGF2CH/A)',1,'官方自营店',322,'苹果','/Storage/Plat/Brand/logo_322.jpg',14,'电脑办公',15,'电脑整机',16,'笔记本','822',0,0,6988.00,'2017-02-14 10:14:59','/Storage/Shop/1/Products/714','\0'),(1814,715,'Apple MacBook Pro 15.4英寸笔记本电脑 银色(Core i7 处理器/16GB内存/256GB SSD闪存/Retina屏 MJLQ2CH/A)',1,'官方自营店',322,'苹果','/Storage/Plat/Brand/logo_322.jpg',14,'电脑办公',15,'电脑整机',16,'笔记本','824',0,0,13688.00,'2017-02-14 10:19:19','/Storage/Shop/1/Products/715','\0'),(1815,716,'宇食俱进厄瓜多尔白虾大虾南美活冻海鲜水产海虾 1700克40-50白虾',1,'官方自营店',323,'大洋世家','/Storage/Plat/Brand/logo_323.jpg',17,'生鲜',18,'海鲜水产',19,'虾类','828,830,835',0,0,158.00,'2017-02-14 10:31:33','/Storage/Shop/1/Products/716',''),(1816,717,'欧莱雅水能润泽双效洁面膏100+50ml',1,'官方自营店',324,'欧莱雅','/Storage/Plat/Brand/logo_324.jpg',20,'个护化妆、清洁用品',21,'面部护肤',22,'洁面','836,839',0,0,19.90,'2017-02-14 10:42:13','/Storage/Shop/1/Products/717','\0'),(1817,718,'贝德玛（Bioderma）深层舒妍卸妆水 舒缓保湿粉水（干性中性敏感肌法国版/海外版随机发）500ml ',1,'官方自营店',328,'贝德玛','/Storage/Plat/Brand/logo_328.jpg',20,'个护化妆、清洁用品',21,'面部护肤',23,'卸妆','837',0,0,110.00,'2017-02-14 10:47:38','/Storage/Shop/1/Products/718','\0'),(1818,719,'资生堂洗颜专科柔澈泡沫洁面乳１２０ｇ',1,'官方自营店',0,NULL,NULL,20,'个护化妆、清洁用品',21,'面部护肤',22,'洁面','837',0,0,37.00,'2017-02-14 14:10:54','/Storage/Shop/1/Products/719',''),(1819,720,'欧莱雅（LOREAL）男士火山岩控油清痘洁面膏100ml',1,'官方自营店',324,'欧莱雅','/Storage/Plat/Brand/logo_324.jpg',20,'个护化妆、清洁用品',21,'面部护肤',22,'洁面','836',0,0,32.90,'2017-02-14 14:13:38','/Storage/Shop/1/Products/720',''),(1820,721,'丝塔芙(Cetaphil)洁面乳118ml',1,'官方自营店',0,NULL,NULL,20,'个护化妆、清洁用品',21,'面部护肤',22,'洁面','839',0,0,55.00,'2017-02-14 14:16:29','/Storage/Shop/1/Products/721',''),(1821,722,'Clinique倩碧液体洁面皂温和型200ml',1,'官方自营店',0,NULL,NULL,20,'个护化妆、清洁用品',21,'面部护肤',22,'洁面','837,838',0,0,110.00,'2017-02-14 14:19:01','/Storage/Shop/1/Products/722',''),(1822,723,'露得清深层净化洗面乳2支装100g*2',1,'官方自营店',0,NULL,NULL,20,'个护化妆、清洁用品',21,'面部护肤',22,'洁面','838',0,0,50.00,'2017-02-14 14:20:57','/Storage/Shop/1/Products/723',''),(1823,724,'2016秋冬新款女装中长款羊毛大衣修身显瘦风衣西装格子毛呢外套女',1,'官方自营店',0,NULL,NULL,8,'男装、女装、童装',9,'女装',10,'外套','819',0,0,248.00,'2017-02-14 14:26:18','/Storage/Shop/1/Products/724','\0'),(1824,725,'2016秋冬新款女装中长款羊毛大衣修身显瘦风衣西装格子毛呢外套女',1,'官方自营店',0,NULL,NULL,8,'男装、女装、童装',9,'女装',10,'外套',NULL,0,0,248.00,'2017-02-14 14:28:50','/Storage/Shop/1/Products/725','\0'),(1825,726,'佐露丝女装2017新款韩版麂皮毛毛绒外套女中长款秋冬保暖加绒大衣宽松风衣潮 浅灰 S',1,'官方自营店',0,NULL,NULL,8,'男装、女装、童装',9,'女装',10,'外套','819',0,0,388.00,'2017-02-14 14:32:42','/Storage/Shop/1/Products/726','\0'),(1826,727,'秀族 2016秋装新款韩版毛呢外套女装宽松粉色大衣中长款',1,'官方自营店',0,NULL,NULL,8,'男装、女装、童装',9,'女装',10,'外套','819',0,0,98.00,'2017-02-14 14:34:49','/Storage/Shop/1/Products/727','\0'),(1827,728,'米兰茵韩版茧型加棉羊毛呢子外套中长款宽松加厚羊绒大衣女装',1,'官方自营店',0,NULL,NULL,8,'男装、女装、童装',9,'女装',10,'外套','819',0,0,189.00,'2017-02-14 14:37:11','/Storage/Shop/1/Products/728','\0'),(1828,729,'2017秋冬加厚韩国韩范宽松中长款毛呢外套显瘦纯色大翻领',1,'官方自营店',0,NULL,NULL,8,'男装、女装、童装',9,'女装',10,'外套','819',0,0,189.00,'2017-02-14 14:41:53','/Storage/Shop/1/Products/729','\0'),(1829,730,'2017秋冬加厚韩国韩范宽松中长款毛呢外套显瘦纯色大翻领',1,'官方自营店',0,NULL,NULL,8,'男装、女装、童装',9,'女装',10,'外套','819',0,0,189.00,'2017-02-14 14:43:51','/Storage/Shop/1/Products/730','\0'),(1830,731,'蔓斯菲尔 设计师椅 简约时尚休闲塑料椅 创意电脑椅子 办公餐椅 会议椅',1,'官方自营店',0,NULL,NULL,25,'家居、家具、家装',35,'家装软式',37,'装饰摆件',NULL,0,0,99.00,'2017-02-14 15:32:54','/Storage/Shop/1/Products/731',''),(1831,732,'全实木电脑桌书桌办公桌',1,'官方自营店',0,NULL,NULL,25,'家居、家具、家装',35,'家装软式',37,'装饰摆件',NULL,0,0,200.00,'2017-02-14 15:37:47','/Storage/Shop/1/Products/732',''),(1832,733,'丹麦依诺维绅 功能沙发床 时尚沙发 书房必用 小鸟',1,'官方自营店',0,NULL,NULL,25,'家居、家具、家装',35,'家装软式',37,'装饰摆件',NULL,0,0,4425.00,'2017-02-14 15:44:52','/Storage/Shop/1/Products/733',''),(1833,734,'原色木质简约现代艺术 家居配饰 软装配饰 可调光 木制DIY蘑菇台灯',1,'官方自营店',0,NULL,NULL,25,'家居、家具、家装',35,'家装软式',37,'装饰摆件',NULL,0,0,63.00,'2017-02-14 15:54:21','/Storage/Shop/1/Products/734',''),(1834,735,'惠宝隆无铅水晶红酒醒酒器蜗牛形干红葡萄酒醒酒壶分酒器500ml',1,'官方自营店',0,NULL,NULL,25,'家居、家具、家装',35,'家装软式',37,'装饰摆件',NULL,0,0,98.00,'2017-02-14 15:59:53','/Storage/Shop/1/Products/735',''),(1835,736,'妮维雅男士水活多效洁面乳100g',1,'官方自营店',0,NULL,NULL,20,'个护化妆、清洁用品',21,'面部护肤',22,'洁面','836',0,0,33.00,'2017-02-14 16:11:39','/Storage/Shop/1/Products/736',''),(1836,737,'怡鲜来 法国新鲜冷冻银鳕鱼中段 200g 进口海鲜水产 深海野生鳕鱼',1,'官方自营店',319,'三只松鼠','/Storage/Plat/Brand/logo_319.jpg',1,'食品、酒类、特产',2,'坚果',7,'碧根果','813',0,0,83.00,'2017-02-14 16:13:13','/Storage/Shop/1/Products/737',''),(1837,738,'妮维雅男士水活畅透精华洁面液150ml',1,'官方自营店',0,NULL,NULL,20,'个护化妆、清洁用品',21,'面部护肤',22,'洁面','836',0,0,35.00,'2017-02-14 16:14:19','/Storage/Shop/1/Products/738',''),(1838,739,'联想（Lenovo）Y50p-70 15.6英寸游戏笔记本电脑（I5-4210H 1T GTX960M升级版 ）',1,'官方自营店',0,NULL,NULL,14,'电脑办公',15,'电脑整机',16,'笔记本',NULL,0,0,4499.00,'2017-02-14 16:17:31','/Storage/Shop/1/Products/739',''),(1839,740,'红霞草莓 3斤 单果20-30克 新鲜水果 SG',1,'官方自营店',0,NULL,NULL,1,'食品、酒类、特产',153,'水果',154,'水果',NULL,0,0,25.00,'2017-02-14 16:18:19','/Storage/Shop/1/Products/740',''),(1840,741,'华硕（ASUS）FX50VX 15.6英寸游戏本（I7-6700 8G 1T GTX950M 2GB Win10 黑红）',1,'官方自营店',0,NULL,NULL,14,'电脑办公',15,'电脑整机',16,'笔记本','824',0,0,5399.00,'2017-02-14 16:20:04','/Storage/Shop/1/Products/741',''),(1841,742,'怡鲜来 阿根廷红虾2000g 4斤/盒 大号L2级 40-60尾',1,'官方自营店',0,NULL,NULL,17,'生鲜',18,'海鲜水产',19,'虾类',NULL,0,0,168.00,'2017-02-14 16:21:08','/Storage/Shop/1/Products/742',''),(1842,743,'勇艺达小勇机器人Y50B 太空银 家庭陪伴 启智教育 声控智能家居 视频监控',1,'官方自营店',0,NULL,NULL,44,'手机、 数码',155,'数码产品',156,'视频类用品',NULL,0,0,2999.00,'2017-02-14 16:27:54','/Storage/Shop/1/Products/743',''),(1843,744,'SK-II 神仙水sk2护肤精华露 160ml',1,'官方自营店',0,NULL,NULL,20,'个护化妆、清洁用品',21,'面部护肤',157,'面部护理','838',0,0,469.00,'2017-02-14 16:37:01','/Storage/Shop/1/Products/744',''),(1844,745,'新西兰蔓越莓蜂蜜480g 进口蜂蜜选新西兰蜂蜜品牌 NatureBeing',1,'官方自营店',0,NULL,NULL,42,'医药保健',127,'营养健康',130,'调节免疫','843,848',0,0,219.00,'2017-02-14 16:42:44','/Storage/Shop/1/Products/745',''),(1845,746,'棉麻布艺禅修加厚加大榻榻米地板圆形坐垫飘窗禅修瑜伽打坐垫55CM',1,'官方自营店',0,NULL,NULL,25,'家居、家具、家装',35,'家装软式',37,'装饰摆件',NULL,0,0,28.00,'2017-02-16 11:24:43','/Storage/Shop/1/Products/746',''),(1846,747,'北欧全实木床 简约美式实木北欧家具套装',1,'官方自营店',0,NULL,NULL,25,'家居、家具、家装',35,'家装软式',37,'装饰摆件',NULL,0,0,1000.00,'2017-02-16 11:35:02','/Storage/Shop/1/Products/747',''),(1847,748,'多功能 无网研磨 易清洗 全钢 304不锈钢 豆浆机',1,'官方自营店',0,NULL,NULL,24,'家用电器',50,'生活电器',67,'加湿器',NULL,0,0,249.00,'2017-02-16 11:46:27','/Storage/Shop/1/Products/748',''),(1848,749,'积家（Jaeger）Master Control大师系列机械男表Q1552520 银色',1,'官方自营店',0,NULL,NULL,151,'钟表',158,'手表',159,'男表',NULL,0,0,20000.00,'2017-02-16 12:01:19','/Storage/Shop/1/Products/749','');
/*!40000 ALTER TABLE `mall_searchproduct` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_sellerspecificationvalue`
--

DROP TABLE IF EXISTS `mall_sellerspecificationvalue`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_sellerspecificationvalue` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `ValueId` bigint(20) NOT NULL COMMENT '规格值ID',
  `Specification` int(11) NOT NULL COMMENT '规格（颜色、尺寸、版本）',
  `TypeId` bigint(20) NOT NULL COMMENT '类型ID',
  `Value` varchar(100) NOT NULL COMMENT '商家的规格值',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_SpecificationValue_SellerSpecificationValue` (`ValueId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=72 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_sellerspecificationvalue`
--

LOCK TABLES `mall_sellerspecificationvalue` WRITE;
/*!40000 ALTER TABLE `mall_sellerspecificationvalue` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_sellerspecificationvalue` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_sendmessagerecord`
--

DROP TABLE IF EXISTS `mall_sendmessagerecord`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_sendmessagerecord` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `MessageType` int(11) NOT NULL COMMENT '消息类别',
  `ContentType` int(11) NOT NULL COMMENT '内容类型',
  `SendContent` varchar(600) NOT NULL COMMENT '发送内容',
  `ToUserLabel` varchar(200) DEFAULT NULL COMMENT '发送对象',
  `SendState` int(11) NOT NULL COMMENT '发送状态',
  `SendTime` datetime NOT NULL COMMENT '发送时间',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=71 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_sendmessagerecord`
--

LOCK TABLES `mall_sendmessagerecord` WRITE;
/*!40000 ALTER TABLE `mall_sendmessagerecord` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_sendmessagerecord` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_sendmessagerecordcoupon`
--

DROP TABLE IF EXISTS `mall_sendmessagerecordcoupon`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_sendmessagerecordcoupon` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `MessageId` bigint(20) NOT NULL,
  `CouponId` bigint(20) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Reference_message` (`MessageId`) USING BTREE,
  KEY `FK_Reference_messageCoupon` (`CouponId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=48 DEFAULT CHARSET=utf8 COMMENT='发送优惠券详细';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_sendmessagerecordcoupon`
--

LOCK TABLES `mall_sendmessagerecordcoupon` WRITE;
/*!40000 ALTER TABLE `mall_sendmessagerecordcoupon` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_sendmessagerecordcoupon` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_sendmessagerecordcouponsn`
--

DROP TABLE IF EXISTS `mall_sendmessagerecordcouponsn`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_sendmessagerecordcouponsn` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `MessageId` bigint(20) NOT NULL,
  `CouponSN` varchar(50) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_sendmessagerecordcouponsn`
--

LOCK TABLES `mall_sendmessagerecordcouponsn` WRITE;
/*!40000 ALTER TABLE `mall_sendmessagerecordcouponsn` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_sendmessagerecordcouponsn` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_sensitiveword`
--

DROP TABLE IF EXISTS `mall_sensitiveword`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_sensitiveword` (
  `Id` int(4) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `SensitiveWord` varchar(100) DEFAULT NULL COMMENT '敏感词',
  `CategoryName` varchar(100) DEFAULT NULL COMMENT '敏感词类别',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `Id` (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_sensitiveword`
--

LOCK TABLES `mall_sensitiveword` WRITE;
/*!40000 ALTER TABLE `mall_sensitiveword` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_sensitiveword` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_settled`
--

DROP TABLE IF EXISTS `mall_settled`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_settled` (
  `ID` bigint(20) NOT NULL AUTO_INCREMENT,
  `BusinessType` int(11) NOT NULL COMMENT '商家类型 0、仅企业可入驻；1、仅个人可入驻；2、企业和个人均可',
  `SettlementAccountType` int(11) NOT NULL COMMENT '商家结算类型 0、仅银行账户；1、仅微信账户；2、银行账户及微信账户均可',
  `TrialDays` int(11) NOT NULL COMMENT '试用天数',
  `IsCity` int(11) NOT NULL COMMENT '地址必填 0、非必填；1、必填',
  `IsPeopleNumber` int(11) NOT NULL COMMENT '人数必填 0、非必填；1、必填',
  `IsAddress` int(11) NOT NULL COMMENT '详细地址必填 0、非必填；1、必填',
  `IsBusinessLicenseCode` int(11) NOT NULL COMMENT '营业执照号必填 0、非必填；1、必填',
  `IsBusinessScope` int(11) NOT NULL COMMENT '经营范围必填 0、非必填；1、必填',
  `IsBusinessLicense` int(11) NOT NULL COMMENT '营业执照必填 0、非必填；1、必填',
  `IsAgencyCode` int(11) NOT NULL COMMENT '机构代码必填 0、非必填；1、必填',
  `IsAgencyCodeLicense` int(11) NOT NULL COMMENT '机构代码证必填 0、非必填；1、必填',
  `IsTaxpayerToProve` int(11) NOT NULL COMMENT '纳税人证明必填 0、非必填；1、必填',
  `CompanyVerificationType` int(11) NOT NULL COMMENT '验证类型 0、验证手机；1、验证邮箱；2、均需验证',
  `IsSName` int(11) NOT NULL COMMENT '个人姓名必填 0、非必填；1、必填',
  `IsSCity` int(11) NOT NULL COMMENT '个人地址必填 0、非必填；1、必填',
  `IsSAddress` int(11) NOT NULL COMMENT '个人详细地址必填 0、非必填；1、必填',
  `IsSIDCard` int(11) NOT NULL COMMENT '个人身份证必填 0、非必填；1、必填',
  `IsSIdCardUrl` int(11) NOT NULL COMMENT '个人身份证上传 0、非必填；1、必填',
  `SelfVerificationType` int(11) NOT NULL COMMENT '个人验证类型 0、验证手机；1、验证邮箱；2、均需验证',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8 COMMENT='入驻设置';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_settled`
--

LOCK TABLES `mall_settled` WRITE;
/*!40000 ALTER TABLE `mall_settled` DISABLE KEYS */;
INSERT INTO `mall_settled` VALUES (2,2,2,7,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1);
/*!40000 ALTER TABLE `mall_settled` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shippingaddress`
--

DROP TABLE IF EXISTS `mall_shippingaddress`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shippingaddress` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `UserId` bigint(20) NOT NULL COMMENT '用户ID',
  `RegionId` int(11) NOT NULL COMMENT '区域ID',
  `ShipTo` varchar(100) NOT NULL COMMENT '收货人',
  `Address` varchar(100) NOT NULL COMMENT '收货具体街道信息',
  `AddressDetail` varchar(100) DEFAULT NULL COMMENT '地址详情(楼栋-门牌)',
  `Phone` varchar(100) NOT NULL COMMENT '收货人电话',
  `IsDefault` tinyint(1) NOT NULL COMMENT '是否为默认',
  `IsQuick` tinyint(1) NOT NULL COMMENT '是否为轻松购地址',
  `Longitude` float NOT NULL DEFAULT '0' COMMENT '经度',
  `Latitude` float NOT NULL DEFAULT '0' COMMENT '纬度',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Member_ShippingAddress` (`UserId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=458 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shippingaddress`
--

LOCK TABLES `mall_shippingaddress` WRITE;
/*!40000 ALTER TABLE `mall_shippingaddress` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shippingaddress` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shippingfreegroup`
--

DROP TABLE IF EXISTS `mall_shippingfreegroup`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shippingfreegroup` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `TemplateId` bigint(20) NOT NULL COMMENT '运费模版ID',
  `ConditionType` int(11) NOT NULL COMMENT '包邮条件类型',
  `ConditionNumber` varchar(100) NOT NULL COMMENT '包邮条件值',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=75 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shippingfreegroup`
--

LOCK TABLES `mall_shippingfreegroup` WRITE;
/*!40000 ALTER TABLE `mall_shippingfreegroup` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shippingfreegroup` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shippingfreeregion`
--

DROP TABLE IF EXISTS `mall_shippingfreeregion`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shippingfreeregion` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `TemplateId` bigint(20) NOT NULL,
  `GroupId` bigint(20) NOT NULL,
  `RegionId` int(11) NOT NULL,
  `RegionPath` varchar(50) DEFAULT NULL COMMENT '地区全路径',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=129 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shippingfreeregion`
--

LOCK TABLES `mall_shippingfreeregion` WRITE;
/*!40000 ALTER TABLE `mall_shippingfreeregion` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shippingfreeregion` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shop`
--

DROP TABLE IF EXISTS `mall_shop`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shop` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `GradeId` bigint(20) NOT NULL COMMENT '店铺等级',
  `ShopName` varchar(100) NOT NULL COMMENT '店铺名称',
  `Logo` varchar(100) DEFAULT NULL COMMENT '店铺LOGO路径',
  `SubDomains` varchar(100) DEFAULT NULL COMMENT '预留子域名，未使用',
  `Theme` varchar(100) DEFAULT NULL COMMENT '预留主题，未使用',
  `IsSelf` tinyint(1) NOT NULL COMMENT '是否是官方自营店',
  `ShopStatus` int(11) NOT NULL COMMENT '店铺状态',
  `RefuseReason` varchar(1000) DEFAULT NULL COMMENT '审核拒绝原因',
  `CreateDate` datetime NOT NULL COMMENT '店铺创建日期',
  `EndDate` datetime NOT NULL COMMENT '店铺过期日期',
  `CompanyName` varchar(100) DEFAULT NULL COMMENT '公司名称',
  `CompanyRegionId` int(11) NOT NULL COMMENT '公司省市区',
  `CompanyAddress` varchar(100) DEFAULT NULL COMMENT '公司地址',
  `CompanyPhone` varchar(100) DEFAULT NULL COMMENT '公司电话',
  `CompanyEmployeeCount` int(11) NOT NULL COMMENT '公司员工数量',
  `CompanyRegisteredCapital` decimal(18,2) NOT NULL COMMENT '公司注册资金',
  `ContactsName` varchar(100) DEFAULT NULL COMMENT '联系人姓名',
  `ContactsPhone` varchar(100) DEFAULT NULL COMMENT '联系电话',
  `ContactsEmail` varchar(100) DEFAULT NULL COMMENT '联系Email',
  `BusinessLicenceNumber` varchar(100) DEFAULT NULL COMMENT '营业执照号',
  `BusinessLicenceNumberPhoto` varchar(100) NOT NULL COMMENT '营业执照',
  `BusinessLicenceRegionId` int(11) NOT NULL COMMENT '营业执照所在地',
  `BusinessLicenceStart` datetime DEFAULT NULL COMMENT '营业执照有效期开始',
  `BusinessLicenceEnd` datetime DEFAULT NULL COMMENT '营业执照有效期',
  `BusinessSphere` varchar(500) DEFAULT NULL COMMENT '法定经营范围',
  `OrganizationCode` varchar(100) DEFAULT NULL COMMENT '组织机构代码',
  `OrganizationCodePhoto` varchar(100) DEFAULT NULL COMMENT '组织机构执照',
  `GeneralTaxpayerPhot` varchar(100) DEFAULT NULL COMMENT '一般纳税人证明',
  `BankAccountName` varchar(100) DEFAULT NULL COMMENT '银行开户名',
  `BankAccountNumber` varchar(100) DEFAULT NULL COMMENT '公司银行账号',
  `BankName` varchar(100) DEFAULT NULL COMMENT '开户银行支行名称',
  `BankCode` varchar(100) DEFAULT NULL COMMENT '支行联行号',
  `BankRegionId` int(11) NOT NULL COMMENT '开户银行所在地',
  `BankPhoto` varchar(100) DEFAULT NULL,
  `TaxRegistrationCertificate` varchar(100) DEFAULT NULL COMMENT '税务登记证',
  `TaxpayerId` varchar(100) DEFAULT NULL COMMENT '税务登记证号',
  `TaxRegistrationCertificatePhoto` varchar(100) DEFAULT NULL COMMENT '纳税人识别号',
  `PayPhoto` varchar(100) DEFAULT NULL COMMENT '支付凭证',
  `PayRemark` varchar(1000) DEFAULT NULL COMMENT '支付注释',
  `SenderName` varchar(100) DEFAULT NULL COMMENT '商家发货人名称',
  `SenderAddress` varchar(100) DEFAULT NULL COMMENT '商家发货人地址',
  `SenderPhone` varchar(100) DEFAULT NULL COMMENT '商家发货人电话',
  `Freight` decimal(18,2) NOT NULL COMMENT '运费',
  `FreeFreight` decimal(18,2) NOT NULL COMMENT '多少钱开始免运费',
  `Stage` int(11) NOT NULL DEFAULT '0' COMMENT '注册步骤',
  `SenderRegionId` int(11) NOT NULL DEFAULT '0' COMMENT '商家发货人省市区',
  `BusinessLicenseCert` varchar(120) DEFAULT NULL COMMENT '营业执照证书',
  `ProductCert` varchar(120) DEFAULT NULL COMMENT '商品证书',
  `OtherCert` varchar(120) DEFAULT NULL COMMENT '其他证书',
  `legalPerson` varchar(50) DEFAULT NULL COMMENT '法人代表',
  `CompanyFoundingDate` datetime DEFAULT NULL COMMENT '公司成立日期',
  `BusinessType` int(11) NOT NULL DEFAULT '0' COMMENT '0、企业；1、个人',
  `IDCard` varchar(50) DEFAULT '' COMMENT '身份证号',
  `IDCardUrl` varchar(200) DEFAULT '' COMMENT '身份证URL',
  `IDCardUrl2` varchar(200) DEFAULT NULL COMMENT '身份证照片URL2',
  `WeiXinNickName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `WeiXinSex` int(11) DEFAULT '0' COMMENT '微信性别;0、男；1、女',
  `WeiXinAddress` varchar(200) DEFAULT '' COMMENT '微信地区',
  `WeiXinTrueName` varchar(200) DEFAULT '' COMMENT '微信真实姓名',
  `WeiXinOpenId` varchar(200) DEFAULT '' COMMENT '微信标识符',
  `WeiXinImg` varchar(200) DEFAULT NULL,
  `AutoAllotOrder` tinyint(1) NOT NULL COMMENT '商家是否开启自动分配订单',
  `IsAutoPrint` bit(1) NOT NULL DEFAULT b'0' COMMENT '商家是否开启自动打印',
  `PrintCount` int(11) NOT NULL DEFAULT '0' COMMENT '打印张数',
  `IsOpenTopImageAd` tinyint(1) NOT NULL DEFAULT '0' COMMENT '是否开启头部图片广告',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `IX_ShopIsSelf` (`IsSelf`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=378 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shop`
--

LOCK TABLES `mall_shop` WRITE;
/*!40000 ALTER TABLE `mall_shop` DISABLE KEYS */;
INSERT INTO `mall_shop` VALUES (1,1,'官方自营店',NULL,NULL,NULL,1,7,NULL,'2014-10-30 00:00:00','2117-02-13 17:05:32','海商网络科技',102,'文化大厦','876588888',1000,1.00,'杨先生','13988887748','','966587458','1',102,'2014-05-05 00:00:00','2014-12-12 00:00:00','1','66548726','1','1','杨先生','6228445888796651200','中国银行','44698',101,'1','1','33695','1','1','1','1','1','1',11.00,11.00,5,102,NULL,NULL,NULL,NULL,NULL,0,'','',NULL,'',0,'','','',NULL,0,'\0',0,0);
/*!40000 ALTER TABLE `mall_shop` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopaccount`
--

DROP TABLE IF EXISTS `mall_shopaccount`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopaccount` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ShopId` bigint(20) NOT NULL COMMENT '店铺Id',
  `ShopName` varchar(100) NOT NULL COMMENT '店铺名称',
  `Balance` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '帐户余额',
  `PendingSettlement` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '待结算',
  `Settled` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '已结算',
  `ReMark` varchar(500) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=53 DEFAULT CHARSET=utf8 COMMENT='店铺资金表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopaccount`
--

LOCK TABLES `mall_shopaccount` WRITE;
/*!40000 ALTER TABLE `mall_shopaccount` DISABLE KEYS */;
INSERT INTO `mall_shopaccount` VALUES (1,1,'官方旗舰店',592.20,0.00,592.20,'');
/*!40000 ALTER TABLE `mall_shopaccount` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopaccountitem`
--

DROP TABLE IF EXISTS `mall_shopaccountitem`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopaccountitem` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `ShopName` varchar(100) NOT NULL COMMENT '店铺名称',
  `AccountNo` varchar(50) NOT NULL COMMENT '交易流水号',
  `AccoutID` bigint(20) NOT NULL COMMENT '关联资金编号',
  `CreateTime` datetime NOT NULL COMMENT '创建时间',
  `Amount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '金额',
  `Balance` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '帐户剩余',
  `TradeType` int(4) NOT NULL DEFAULT '0' COMMENT '交易类型',
  `IsIncome` bit(1) NOT NULL COMMENT '是否收入',
  `ReMark` varchar(1000) DEFAULT NULL COMMENT '交易备注',
  `DetailId` varchar(100) DEFAULT NULL COMMENT '详情ID',
  `SettlementCycle` int(11) NOT NULL COMMENT '结算周期(以天为单位)(冗余字段)',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_mall_Shtem_AccoutID` (`AccoutID`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=396 DEFAULT CHARSET=utf8 COMMENT='店铺资金流水表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopaccountitem`
--

LOCK TABLES `mall_shopaccountitem` WRITE;
/*!40000 ALTER TABLE `mall_shopaccountitem` DISABLE KEYS */;
INSERT INTO `mall_shopaccountitem` VALUES (68,1,'官方自营店','11017696',1,'2018-05-14 11:21:28',0.00,0.00,5,'\0','店铺购买拼团服务,12个月','101',0),(69,1,'官方自营店','11024338',1,'2018-05-14 11:21:37',0.00,0.00,5,'\0','店铺购买拼团服务,12个月','102',0),(70,1,'官方自营店','11038676',1,'2018-05-14 11:22:31',0.00,0.00,5,'\0','店铺购买随机红包服务,12个月','103',0),(71,1,'官方自营店','11049583',1,'2018-05-14 11:35:34',0.00,0.00,5,'\0','店铺购买限时购服务,1个月','104',0),(72,1,'官方自营店','11054374',1,'2018-05-14 11:38:30',0.00,0.00,5,'\0','店铺购买优惠券服务,12个月','105',0),(73,1,'官方自营店','11061384',1,'2018-05-14 11:55:57',0.00,0.00,5,'\0','店铺购买组合购服务,12个月','106',0),(74,1,'官方自营店','11078156',1,'2018-05-14 11:56:02',0.00,0.00,5,'\0','店铺购买组合购服务,12个月','107',0),(75,1,'官方旗舰店','201702150050002697807',1,'2018-05-15 00:50:00',592.20,592.20,1,'','店铺结算明细157','157',1);
/*!40000 ALTER TABLE `mall_shopaccountitem` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopbonus`
--

DROP TABLE IF EXISTS `mall_shopbonus`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopbonus` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL,
  `Name` varchar(40) NOT NULL,
  `Count` int(11) NOT NULL COMMENT '红包数量',
  `RandomAmountStart` decimal(18,2) NOT NULL COMMENT '随机范围Start',
  `RandomAmountEnd` decimal(18,2) NOT NULL COMMENT '随机范围End',
  `UseState` int(11) NOT NULL COMMENT '1:满X元使用  2：没有限制',
  `UsrStatePrice` decimal(18,2) NOT NULL COMMENT '满多少元',
  `GrantPrice` decimal(18,2) NOT NULL COMMENT '满多少元才发放红包',
  `DateStart` datetime NOT NULL,
  `DateEnd` datetime NOT NULL,
  `BonusDateStart` datetime NOT NULL,
  `BonusDateEnd` datetime NOT NULL,
  `ShareTitle` varchar(30) NOT NULL COMMENT '分享',
  `ShareDetail` varchar(150) NOT NULL COMMENT '分享',
  `ShareImg` varchar(200) NOT NULL COMMENT '分享',
  `SynchronizeCard` tinyint(1) NOT NULL COMMENT '是否同步到微信卡包，是的话才出现微信卡卷相关UI',
  `CardTitle` varchar(30) DEFAULT NULL COMMENT '微信卡卷相关',
  `CardColor` varchar(20) DEFAULT NULL COMMENT '微信卡卷相关',
  `CardSubtitle` varchar(30) DEFAULT NULL COMMENT '微信卡卷相关',
  `IsInvalid` tinyint(1) NOT NULL COMMENT '是否失效',
  `ReceiveCount` int(11) NOT NULL COMMENT '领取数量',
  `QRPath` varchar(80) NOT NULL COMMENT '二维码路径',
  `WXCardState` int(255) NOT NULL COMMENT '微信卡卷审核状态',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_zzzShopId` (`ShopId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopbonus`
--

LOCK TABLES `mall_shopbonus` WRITE;
/*!40000 ALTER TABLE `mall_shopbonus` DISABLE KEYS */;
INSERT INTO `mall_shopbonus` VALUES (8,1,'情人节领红包',520,1.00,5.20,2,52.00,0.00,'2017-02-14 00:00:00','2017-02-15 23:59:59','2017-02-15 00:00:00','2017-04-01 23:59:59','情人节只送红包','情人节领取52元钱红包','/Storage/Shop/Bonus/61cd62c4-18ca-45f2-8377-62b4bec37ed5.png',1,'情人节红包','Color101',NULL,0,0,'',0);
/*!40000 ALTER TABLE `mall_shopbonus` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopbonusgrant`
--

DROP TABLE IF EXISTS `mall_shopbonusgrant`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopbonusgrant` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopBonusId` bigint(20) NOT NULL COMMENT '红包Id',
  `UserId` bigint(20) NOT NULL COMMENT '发放人',
  `OrderId` bigint(20) NOT NULL,
  `BonusQR` varchar(255) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_ShopBonusId` (`ShopBonusId`) USING BTREE,
  KEY `FK_zzzUserID` (`UserId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopbonusgrant`
--

LOCK TABLES `mall_shopbonusgrant` WRITE;
/*!40000 ALTER TABLE `mall_shopbonusgrant` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shopbonusgrant` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopbonusreceive`
--

DROP TABLE IF EXISTS `mall_shopbonusreceive`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopbonusreceive` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `BonusGrantId` bigint(20) NOT NULL COMMENT '红包Id',
  `OpenId` varchar(100) DEFAULT NULL,
  `Price` decimal(18,2) NOT NULL COMMENT '面额',
  `State` int(11) NOT NULL COMMENT '1.未使用  2.已使用  3.已过期',
  `ReceiveTime` datetime NOT NULL COMMENT '领取时间',
  `UsedTime` datetime DEFAULT NULL COMMENT '使用时间',
  `UserId` bigint(20) NOT NULL DEFAULT '0' COMMENT 'UserID',
  `UsedOrderId` bigint(20) DEFAULT NULL COMMENT '使用的订单号',
  `WXName` varchar(30) DEFAULT NULL,
  `WXHead` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_BonusGrantId` (`BonusGrantId`) USING BTREE,
  KEY `FK_useUserID` (`UserId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopbonusreceive`
--

LOCK TABLES `mall_shopbonusreceive` WRITE;
/*!40000 ALTER TABLE `mall_shopbonusreceive` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shopbonusreceive` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopbranch`
--

DROP TABLE IF EXISTS `mall_shopbranch`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopbranch` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '主键',
  `ShopId` bigint(20) NOT NULL COMMENT '商家店铺ID',
  `ShopBranchName` varchar(30) NOT NULL COMMENT '门店名称',
  `AddressId` int(11) NOT NULL COMMENT '门店地址ID',
  `AddressPath` varchar(50) DEFAULT NULL COMMENT '所在区域全路径编号(省，市，区)',
  `AddressDetail` varchar(100) DEFAULT NULL,
  `ContactUser` varchar(50) NOT NULL COMMENT '联系人',
  `ContactPhone` varchar(50) NOT NULL COMMENT '联系地址',
  `Status` int(11) NOT NULL COMMENT '门店状态(0:正常，1:冻结)',
  `CreateDate` datetime NOT NULL COMMENT '创建时间',
  `ServeRadius` int(11) NOT NULL DEFAULT '0' COMMENT '服务半径',
  `Longitude` float NOT NULL DEFAULT '0' COMMENT '经度',
  `Latitude` float NOT NULL DEFAULT '0' COMMENT '维度',
  `ShopImages` varchar(500) DEFAULT NULL,
  `IsStoreDelive` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否门店配送0:否1:是',
  `IsAboveSelf` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否上门自提0:否1:是',
  `StoreOpenStartTime` time NOT NULL DEFAULT '08:00:00' COMMENT '营业起始时间',
  `StoreOpenEndTime` time NOT NULL DEFAULT '20:00:00' COMMENT '营业结束时间',
  `IsRecommend` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否推荐门店',
  `RecommendSequence` bigint(20) NOT NULL DEFAULT '0' COMMENT '推荐排序',
  `DeliveFee` int(11) NOT NULL DEFAULT '0' COMMENT '配送费',
  `DeliveTotalFee` int(11) NOT NULL DEFAULT '0' COMMENT '起送费',
  `FreeMailFee` int(11) NOT NULL DEFAULT '0' COMMENT '包邮金额',
  `DaDaShopId` varchar(100) DEFAULT NULL,
  `IsAutoPrint` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否开启自动打印',
  `PrintCount` int(11) NOT NULL DEFAULT '0' COMMENT '打印张数',
  `IsFreeMail` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否包邮',
  `EnableSellerManager` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否允许商城越权',
  `IsShelvesProduct` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否自动上架商品(0:否、1:是)',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=128 DEFAULT CHARSET=utf8 COMMENT='门店信息表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopbranch`
--

LOCK TABLES `mall_shopbranch` WRITE;
/*!40000 ALTER TABLE `mall_shopbranch` DISABLE KEYS */;
INSERT INTO `mall_shopbranch` VALUES (26,1,'辣条君',27073,'1812,1813,1814,27073,','湖南省长沙市韶山北路139号湖南文化大厦','李广田','13306589754',0,'0001-01-01 00:00:00',0,0,0,NULL,'\0','\0','08:00:00','20:00:00','\0',0,0,0,0,NULL,'\0',0,'\0','\0','\0');
/*!40000 ALTER TABLE `mall_shopbranch` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopbranchintag`
--

DROP TABLE IF EXISTS `mall_shopbranchintag`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopbranchintag` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopBranchId` bigint(20) NOT NULL COMMENT '门店管理ID',
  `ShopBranchTagId` bigint(20) NOT NULL COMMENT '门店标签关联ID',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_ShopBranchId` (`ShopBranchId`) USING BTREE,
  KEY `FK_ShopBranchTagId` (`ShopBranchTagId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=460 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopbranchintag`
--

LOCK TABLES `mall_shopbranchintag` WRITE;
/*!40000 ALTER TABLE `mall_shopbranchintag` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shopbranchintag` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopbranchmanager`
--

DROP TABLE IF EXISTS `mall_shopbranchmanager`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopbranchmanager` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopBranchId` bigint(20) NOT NULL COMMENT '门店表ID',
  `UserName` varchar(100) NOT NULL COMMENT '用户名称',
  `Password` varchar(100) NOT NULL COMMENT '密码',
  `PasswordSalt` varchar(100) NOT NULL COMMENT '密码加盐',
  `CreateDate` datetime NOT NULL COMMENT '创建日期',
  `Remark` varchar(1000) DEFAULT NULL,
  `RealName` varchar(1000) DEFAULT NULL COMMENT '真实名称',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=137 DEFAULT CHARSET=utf8 COMMENT='门店管理员表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopbranchmanager`
--

LOCK TABLES `mall_shopbranchmanager` WRITE;
/*!40000 ALTER TABLE `mall_shopbranchmanager` DISABLE KEYS */;
INSERT INTO `mall_shopbranchmanager` VALUES (22,26,'admin','75cf2238a251d615ec8b7727a481e3a5','4b0586411be3e8d74f93','2017-02-14 10:25:38',NULL,NULL);
/*!40000 ALTER TABLE `mall_shopbranchmanager` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopbranchsku`
--

DROP TABLE IF EXISTS `mall_shopbranchsku`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopbranchsku` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ProductId` bigint(20) NOT NULL COMMENT '商品id(冗余字段)',
  `SkuId` varchar(100) NOT NULL COMMENT 'SKU表Id',
  `ShopId` bigint(20) NOT NULL COMMENT '商家id(冗余字段)',
  `ShopBranchId` bigint(20) NOT NULL COMMENT '门店id',
  `Stock` int(11) NOT NULL COMMENT '库存',
  `Status` int(11) NOT NULL COMMENT '门店SKU状态',
  `CreateDate` datetime NOT NULL COMMENT 'SKU添加时间',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=1133 DEFAULT CHARSET=utf8 COMMENT='商家分店SKU信息';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopbranchsku`
--

LOCK TABLES `mall_shopbranchsku` WRITE;
/*!40000 ALTER TABLE `mall_shopbranchsku` DISABLE KEYS */;
INSERT INTO `mall_shopbranchsku` VALUES (24,702,'702_0_0_0',1,26,1000,0,'2017-02-14 14:53:29'),(25,704,'704_0_0_0',1,26,1000,0,'2017-02-14 14:53:29'),(26,705,'705_0_0_0',1,26,995,0,'2017-02-14 14:53:29'),(27,706,'706_0_0_0',1,26,1000,1,'2017-02-14 14:53:29'),(28,707,'707_655_663_0',1,26,1000,1,'2017-02-14 14:53:29'),(29,701,'701_0_0_0',1,26,994,0,'2017-02-14 14:53:29'),(30,700,'700_0_0_0',1,26,990,0,'2017-02-14 14:53:29'),(31,716,'716_0_0_0',1,26,1000,1,'2017-02-14 14:53:29'),(32,708,'708_0_0_0',1,26,1000,1,'2017-02-14 14:53:29');
/*!40000 ALTER TABLE `mall_shopbranchsku` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopbranchtag`
--

DROP TABLE IF EXISTS `mall_shopbranchtag`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopbranchtag` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '门店标签ID',
  `Title` varchar(30) DEFAULT NULL COMMENT '标签名称',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopbranchtag`
--

LOCK TABLES `mall_shopbranchtag` WRITE;
/*!40000 ALTER TABLE `mall_shopbranchtag` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shopbranchtag` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopbrand`
--

DROP TABLE IF EXISTS `mall_shopbrand`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopbrand` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ShopId` bigint(20) NOT NULL COMMENT '商家Id',
  `BrandId` bigint(20) NOT NULL COMMENT '品牌Id',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `ShopId` (`ShopId`) USING BTREE,
  KEY `BrandId` (`BrandId`) USING BTREE,
  KEY `Id` (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=200 DEFAULT CHARSET=gbk;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopbrand`
--

LOCK TABLES `mall_shopbrand` WRITE;
/*!40000 ALTER TABLE `mall_shopbrand` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shopbrand` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopbrandapply`
--

DROP TABLE IF EXISTS `mall_shopbrandapply`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopbrandapply` (
  `Id` int(11) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ShopId` bigint(20) NOT NULL COMMENT '商家Id',
  `BrandId` bigint(20) NOT NULL COMMENT '品牌Id',
  `BrandName` varchar(100) DEFAULT NULL COMMENT '品牌名称',
  `Logo` varchar(1000) DEFAULT NULL COMMENT '品牌Logo',
  `Description` varchar(1000) DEFAULT NULL COMMENT '描述',
  `AuthCertificate` varchar(4000) DEFAULT NULL COMMENT '品牌授权证书',
  `ApplyMode` int(11) NOT NULL COMMENT '申请类型 枚举 BrandApplyMode',
  `Remark` varchar(1000) DEFAULT NULL COMMENT '备注',
  `AuditStatus` int(11) NOT NULL COMMENT '审核状态 枚举 BrandAuditStatus',
  `ApplyTime` datetime NOT NULL COMMENT '操作时间',
  `PlatRemark` varchar(255) DEFAULT NULL COMMENT '平台备注',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_ShopId` (`ShopId`) USING BTREE,
  KEY `FK_BrandId` (`BrandId`) USING BTREE,
  KEY `Id` (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=281 DEFAULT CHARSET=gbk;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopbrandapply`
--

LOCK TABLES `mall_shopbrandapply` WRITE;
/*!40000 ALTER TABLE `mall_shopbrandapply` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shopbrandapply` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopcategory`
--

DROP TABLE IF EXISTS `mall_shopcategory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopcategory` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `ParentCategoryId` bigint(20) NOT NULL COMMENT '上级分类ID',
  `Name` varchar(100) DEFAULT NULL COMMENT '分类名称',
  `DisplaySequence` bigint(20) NOT NULL,
  `IsShow` tinyint(1) NOT NULL COMMENT '是否显示',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=456 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopcategory`
--

LOCK TABLES `mall_shopcategory` WRITE;
/*!40000 ALTER TABLE `mall_shopcategory` DISABLE KEYS */;
INSERT INTO `mall_shopcategory` VALUES (350,1,0,'衣服',1,1),(351,1,350,'女装',1,1),(352,1,0,'零食',2,1),(353,1,352,'松子',1,1),(354,1,352,'核桃',2,1),(355,1,0,'生鲜',3,1),(356,1,355,'虾类',1,1),(357,1,0,'电脑',4,1),(358,1,357,'笔记本',1,1),(359,1,0,'个护',5,1),(360,1,359,'洁面/卸妆',1,1);
/*!40000 ALTER TABLE `mall_shopcategory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopfooter`
--

DROP TABLE IF EXISTS `mall_shopfooter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopfooter` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL,
  `Footer` varchar(5000) DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=40 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopfooter`
--

LOCK TABLES `mall_shopfooter` WRITE;
/*!40000 ALTER TABLE `mall_shopfooter` DISABLE KEYS */;
INSERT INTO `mall_shopfooter` VALUES (19,1,'');
/*!40000 ALTER TABLE `mall_shopfooter` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopgrade`
--

DROP TABLE IF EXISTS `mall_shopgrade`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopgrade` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL COMMENT '店铺等级名称',
  `ProductLimit` int(11) NOT NULL COMMENT '最大上传商品数量',
  `ImageLimit` int(11) NOT NULL COMMENT '最大图片可使用空间数量',
  `TemplateLimit` int(11) NOT NULL,
  `ChargeStandard` decimal(8,2) NOT NULL,
  `Remark` varchar(1000) DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopgrade`
--

LOCK TABLES `mall_shopgrade` WRITE;
/*!40000 ALTER TABLE `mall_shopgrade` DISABLE KEYS */;
INSERT INTO `mall_shopgrade` VALUES (1,'白金店铺',500,500,500,500.00,NULL),(2,'钻石店铺',1000,1000,1000,1000.00,NULL);
/*!40000 ALTER TABLE `mall_shopgrade` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shophomemodule`
--

DROP TABLE IF EXISTS `mall_shophomemodule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shophomemodule` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL,
  `Name` varchar(20) NOT NULL COMMENT '模块名称',
  `IsEnable` tinyint(1) NOT NULL COMMENT '是否启用',
  `DisplaySequence` int(11) NOT NULL COMMENT '排序',
  `Url` varchar(200) DEFAULT NULL COMMENT '楼层链接',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shophomemodule`
--

LOCK TABLES `mall_shophomemodule` WRITE;
/*!40000 ALTER TABLE `mall_shophomemodule` DISABLE KEYS */;
INSERT INTO `mall_shophomemodule` VALUES (29,1,'服装',1,3,'./Search/SearchAd?cid=9'),(30,1,'清洁护肤',1,2,'./Search/SearchAd?cid=35'),(31,1,'家居',1,1,'./Search/SearchAd?');
/*!40000 ALTER TABLE `mall_shophomemodule` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shophomemoduleproduct`
--

DROP TABLE IF EXISTS `mall_shophomemoduleproduct`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shophomemoduleproduct` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `HomeModuleId` bigint(20) NOT NULL,
  `ProductId` bigint(20) NOT NULL COMMENT '商品ID',
  `DisplaySequence` int(11) NOT NULL COMMENT '排序',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Product_ShopHomeModuleProduct` (`ProductId`) USING BTREE,
  KEY `FK_ShopHomeModule_ShopHomeModuleProduct` (`HomeModuleId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=108 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shophomemoduleproduct`
--

LOCK TABLES `mall_shophomemoduleproduct` WRITE;
/*!40000 ALTER TABLE `mall_shophomemoduleproduct` DISABLE KEYS */;
INSERT INTO `mall_shophomemoduleproduct` VALUES (48,29,705,1),(49,29,706,2),(50,29,707,3),(51,29,708,4),(84,30,719,9),(85,30,720,10),(86,30,721,11),(87,30,722,12),(88,30,723,13),(89,30,736,18),(90,30,738,19),(91,30,744,21),(100,31,731,5),(101,31,732,6),(102,31,733,7),(103,31,734,8),(104,31,743,9),(105,31,746,10),(106,31,747,11),(107,31,748,12);
/*!40000 ALTER TABLE `mall_shophomemoduleproduct` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shophomemoduletopimg`
--

DROP TABLE IF EXISTS `mall_shophomemoduletopimg`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shophomemoduletopimg` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ImgPath` varchar(200) NOT NULL,
  `Url` varchar(200) DEFAULT NULL,
  `HomeModuleId` bigint(20) NOT NULL,
  `DisplaySequence` int(11) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_SFTHomeModuleId` (`HomeModuleId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shophomemoduletopimg`
--

LOCK TABLES `mall_shophomemoduletopimg` WRITE;
/*!40000 ALTER TABLE `mall_shophomemoduletopimg` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shophomemoduletopimg` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopinvoiceconfig`
--

DROP TABLE IF EXISTS `mall_shopinvoiceconfig`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopinvoiceconfig` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL DEFAULT '0' COMMENT '商家ID',
  `IsInvoice` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否提供发票',
  `IsPlainInvoice` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否提供普通发票',
  `IsElectronicInvoice` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否提供电子发票',
  `PlainInvoiceRate` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '普通发票税率',
  `IsVatInvoice` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否提供增值税发票',
  `VatInvoiceDay` int(11) NOT NULL DEFAULT '0' COMMENT '订单完成后多少天开具增值税发票',
  `VatInvoiceRate` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '增值税税率',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopinvoiceconfig`
--

LOCK TABLES `mall_shopinvoiceconfig` WRITE;
/*!40000 ALTER TABLE `mall_shopinvoiceconfig` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shopinvoiceconfig` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopopenapisetting`
--

DROP TABLE IF EXISTS `mall_shopopenapisetting`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopopenapisetting` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '主键',
  `ShopId` bigint(20) NOT NULL COMMENT '店铺编号',
  `AppKey` varchar(100) NOT NULL COMMENT 'app_key',
  `AppSecreat` varchar(100) NOT NULL COMMENT 'app_secreat',
  `AddDate` datetime NOT NULL COMMENT '增加时间',
  `LastEditDate` datetime NOT NULL COMMENT '最后重置时间',
  `IsEnable` tinyint(1) NOT NULL DEFAULT '0' COMMENT '是否开启',
  `IsRegistered` tinyint(1) NOT NULL DEFAULT '0' COMMENT '是否已注册',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopopenapisetting`
--

LOCK TABLES `mall_shopopenapisetting` WRITE;
/*!40000 ALTER TABLE `mall_shopopenapisetting` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shopopenapisetting` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shoppingcart`
--

DROP TABLE IF EXISTS `mall_shoppingcart`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shoppingcart` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `UserId` bigint(20) NOT NULL COMMENT '用户ID',
  `ShopBranchId` bigint(20) NOT NULL DEFAULT '0' COMMENT '门店ID',
  `ProductId` bigint(20) NOT NULL COMMENT '商品ID',
  `SkuId` varchar(100) DEFAULT NULL COMMENT 'SKUID',
  `Quantity` bigint(20) NOT NULL COMMENT '购买数量',
  `AddTime` datetime NOT NULL COMMENT '添加时间',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Member_ShoppingCart` (`UserId`) USING BTREE,
  KEY `FK_Product_ShoppingCart` (`ProductId`) USING BTREE,
  KEY `mall_shoppingcarts_ibfk_3` (`ShopBranchId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2904 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shoppingcart`
--

LOCK TABLES `mall_shoppingcart` WRITE;
/*!40000 ALTER TABLE `mall_shoppingcart` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shoppingcart` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shoprenewrecord`
--

DROP TABLE IF EXISTS `mall_shoprenewrecord`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shoprenewrecord` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `Operator` varchar(100) NOT NULL COMMENT '操作者',
  `OperateDate` datetime NOT NULL COMMENT '操作日期',
  `OperateContent` varchar(1000) DEFAULT NULL COMMENT '操作明细',
  `OperateType` int(1) NOT NULL COMMENT '类型',
  `Amount` decimal(10,2) NOT NULL COMMENT '支付金额',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8 COMMENT='店铺续费记录表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shoprenewrecord`
--

LOCK TABLES `mall_shoprenewrecord` WRITE;
/*!40000 ALTER TABLE `mall_shoprenewrecord` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shoprenewrecord` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopshipper`
--

DROP TABLE IF EXISTS `mall_shopshipper`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopshipper` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL COMMENT '商家编号',
  `IsDefaultSendGoods` tinyint(1) NOT NULL DEFAULT '0' COMMENT '是否为默认发货地址',
  `IsDefaultGetGoods` tinyint(1) NOT NULL DEFAULT '0' COMMENT '是否默认收货地址',
  `IsDefaultVerification` tinyint(1) DEFAULT '0' COMMENT '默认核销地址',
  `ShipperTag` varchar(100) NOT NULL DEFAULT '' COMMENT '发货点名称',
  `ShipperName` varchar(100) NOT NULL DEFAULT '' COMMENT '发货人',
  `RegionId` int(11) NOT NULL DEFAULT '0' COMMENT '区域ID',
  `Address` varchar(300) NOT NULL DEFAULT '' COMMENT '具体街道信息',
  `TelPhone` varchar(20) DEFAULT '' COMMENT '手机号码',
  `Zipcode` varchar(20) DEFAULT '',
  `WxOpenId` varchar(128) DEFAULT '' COMMENT '微信OpenID用于发信息到微信给发货人',
  `Longitude` float NOT NULL DEFAULT '0' COMMENT '经度',
  `Latitude` float NOT NULL DEFAULT '0' COMMENT '纬度',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopshipper`
--

LOCK TABLES `mall_shopshipper` WRITE;
/*!40000 ALTER TABLE `mall_shopshipper` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shopshipper` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopvisti`
--

DROP TABLE IF EXISTS `mall_shopvisti`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopvisti` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `ShopBranchId` bigint(20) DEFAULT '0' COMMENT '门店编号',
  `Date` datetime NOT NULL COMMENT '日期',
  `VistiCounts` bigint(20) NOT NULL COMMENT '浏览人数',
  `OrderUserCount` bigint(20) NOT NULL COMMENT '下单人数',
  `OrderCount` bigint(20) NOT NULL COMMENT '订单数',
  `OrderProductCount` bigint(20) NOT NULL COMMENT '下单件数',
  `OrderAmount` decimal(18,2) NOT NULL COMMENT '下单金额',
  `OrderPayUserCount` bigint(20) NOT NULL COMMENT '下单付款人数',
  `OrderPayCount` bigint(20) NOT NULL COMMENT '付款订单数',
  `SaleCounts` bigint(20) NOT NULL COMMENT '付款下单件数',
  `SaleAmounts` decimal(18,2) NOT NULL COMMENT '付款金额',
  `OrderRefundProductCount` bigint(20) NOT NULL DEFAULT '0' COMMENT '退款件数',
  `OrderRefundAmount` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '退款金额',
  `OrderRefundCount` bigint(20) NOT NULL DEFAULT '0' COMMENT '退款订单数',
  `UnitPrice` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '件单价',
  `JointRate` decimal(18,2) NOT NULL DEFAULT '0.00' COMMENT '连带率',
  `StatisticFlag` bit(1) NOT NULL COMMENT '是否已经统计(0：未统计,1已统计)',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=4081 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopvisti`
--

LOCK TABLES `mall_shopvisti` WRITE;
/*!40000 ALTER TABLE `mall_shopvisti` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shopvisti` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopwdgjsetting`
--

DROP TABLE IF EXISTS `mall_shopwdgjsetting`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopwdgjsetting` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '主键',
  `ShopId` bigint(20) NOT NULL COMMENT '店铺Id',
  `uCode` varchar(255) NOT NULL COMMENT '接入码',
  `uSign` varchar(255) NOT NULL COMMENT '效验码',
  `IsEnable` tinyint(1) NOT NULL DEFAULT '0' COMMENT '是否开启',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopwdgjsetting`
--

LOCK TABLES `mall_shopwdgjsetting` WRITE;
/*!40000 ALTER TABLE `mall_shopwdgjsetting` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shopwdgjsetting` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_shopwithdraw`
--

DROP TABLE IF EXISTS `mall_shopwithdraw`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_shopwithdraw` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `CashNo` varchar(100) NOT NULL COMMENT '提现流水号',
  `ApplyTime` datetime NOT NULL COMMENT '申请时间',
  `Status` int(11) NOT NULL COMMENT '提现状态',
  `CashType` int(11) NOT NULL COMMENT '提现方式',
  `CashAmount` decimal(18,2) NOT NULL COMMENT '提现金额',
  `Account` varchar(100) NOT NULL COMMENT '提现帐号',
  `AccountName` varchar(100) NOT NULL COMMENT '提现人',
  `SellerId` bigint(20) NOT NULL,
  `SellerName` varchar(100) NOT NULL COMMENT '申请商家用户名',
  `DealTime` datetime DEFAULT NULL COMMENT '处理时间',
  `ShopRemark` varchar(1000) DEFAULT NULL COMMENT '商家备注',
  `PlatRemark` varchar(1000) DEFAULT NULL COMMENT '平台备注',
  `ShopName` varchar(200) DEFAULT '' COMMENT '商店名称',
  `SerialNo` varchar(200) DEFAULT '' COMMENT '支付商流水号',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=30 DEFAULT CHARSET=utf8 COMMENT='店铺提现表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_shopwithdraw`
--

LOCK TABLES `mall_shopwithdraw` WRITE;
/*!40000 ALTER TABLE `mall_shopwithdraw` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_shopwithdraw` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_sitesetting`
--

DROP TABLE IF EXISTS `mall_sitesetting`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_sitesetting` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Key` varchar(100) NOT NULL,
  `Value` varchar(4000) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=231 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_sitesetting`
--

LOCK TABLES `mall_sitesetting` WRITE;
/*!40000 ALTER TABLE `mall_sitesetting` DISABLE KEYS */;
INSERT INTO `mall_sitesetting` VALUES (1,'Logo','/Storage/Plat/Site/logo.png'),(2,'SiteName','mall'),(3,'ICPNubmer',' '),(4,'CustomerTel',' '),(6,'Keyword','实木床'),(7,'Hotkeywords','三只松鼠,实木床'),(8,'PageFoot',' '),(10,'MemberLogo','/Storage/Plat/Site/memberLogo.png'),(11,'QRCode','/Storage/Plat/Site/qrCode.png'),(12,'FlowScript','FlowScript1'),(13,'Site_SEOTitle','mall多用户商城'),(14,'Site_SEOKeywords','mall,b2b2c商城'),(15,'Site_SEODescription','mall,b2b2c商城'),(16,'ProdutAuditOnOff','1'),(17,'WithDrawMinimum','1'),(18,'WithDrawMaximum','2000'),(19,'WeekSettlement','1'),(97,'SiteIsClose','False'),(98,'RegisterType','0'),(99,'MobileVerifOpen','False'),(100,'RegisterEmailRequired','False'),(101,'EmailVerifOpen','False'),(102,'WXLogo','/Storage/Plat/Site/wxlogo.png'),(103,'PCLoginPic','/Storage/Plat/Site/pcloginpic.png'),(104,'PCBottomPic','/Storage/Plat/ImageAd/PCBottomPic.jpg'),(105,'WeixinAppId','dsfsd'),(106,'WeixinAppSecret','k'),(107,'WeixinToken','2A11E2FE9FFBA3A1'),(108,'WeixinPartnerID',''),(109,'WeixinPartnerKey',''),(110,'WeixinLoginUrl',''),(111,'WeixinIsValidationService','False'),(112,'SellerAdminAgreement',''),(113,'AdvancePaymentPercent','0'),(114,'AdvancePaymentLimit','0'),(115,'UnpaidTimeout','1'),(116,'NoReceivingTimeout','9'),(117,'OrderCommentTimeout','15'),(118,'SalesReturnTimeout','15'),(119,'AS_ShopConfirmTimeout','1'),(120,'AS_SendGoodsCloseTimeout','1'),(121,'AS_ShopNoReceivingTimeout','1'),(122,'WX_MSGGetCouponTemplateId',''),(123,'AppUpdateDescription',''),(124,'AppVersion','2.5'),(125,'AndriodDownLoad','./app/mall.apk'),(126,'IOSDownLoad','https://itunes.apple.com/cn/app/id1058273436'),(127,'CanDownload','True'),(128,'ShopAppVersion',''),(129,'ShopAndriodDownLoad',''),(130,'ShopIOSDownLoad',''),(131,'KuaidiType','0'),(132,'Kuaidi100Key',''),(133,'KuaidiApp_key',''),(134,'KuaidiAppSecret',''),(135,'Limittime','True'),(136,'AdvertisementImagePath',''),(137,'AdvertisementUrl',''),(138,'AdvertisementState','False'),(139,'IsOpenStore','True'),(140,'IsOpenShopApp','True'),(141,'WeixinAppletId',''),(142,'WeixinAppletSecret',''),(143,'IsOpenPC','True'),(144,'IsOpenH5','True'),(145,'IsOpenApp','True'),(146,'IsOpenMallSmallProg','True'),(150,'mallJDVersion','0.0.0'),(195,'IntegralDeductibleRate','100'),(196,'ShopWithDrawMinimum','1'),(197,'ShopWithDrawMaximum','5000'),(198,'IsCanClearDemoData','True'),(199,'UserCookieKey','c1e570a9e7010f01da92306c2ad2447c'),(200,'ProductSaleCountOnOff','0'),(201,'AutoAllotOrder','False'),(202,'XcxHomeVersionCode','0'),(203,'Withdraw_AlipayEnable','False'),(204,'LimitTimeBuyNeedAuditing','False'),(205,'IsOpenHistoryOrder','False'),(206,'IsOpenRechargePresent','False'),(207,'IsOpenWithdraw','False'),(208,'IsConBindCellPhone','False'),(209,'DistributionIsEnable','False'),(210,'DistributionMaxLevel','1'),(211,'DistributionMaxBrokerageRate','0'),(212,'DistributionIsProductShowTips','False'),(213,'DistributionCanSelfBuy','False'),(214,'DistributorNeedAudit','False'),(215,'DistributorApplyNeedQuota','0'),(216,'DistributorRenameOpenMyShop','我要开店'),(217,'DistributorRenameMyShop','我的小店'),(218,'DistributorRenameSpreadShop','推广小店'),(219,'DistributorRenameBrokerage','佣金'),(220,'DistributorRenameMarket','分销市场'),(221,'DistributorRenameShopOrder','小店订单'),(222,'DistributorRenameMyBrokerage','我的佣金'),(223,'DistributorRenameMySubordinate','我的下级'),(224,'DistributorRenameMemberLevel1','一级会员'),(225,'DistributorRenameMemberLevel2','二级会员'),(226,'DistributorRenameMemberLevel3','三级会员'),(227,'DistributorRenameShopConfig','小店设置'),(228,'DistributorWithdrawMinLimit','1'),(229,'DistributorWithdrawMaxLimit','2000'),(230,'DistributorWithdrawTypes','capital');
/*!40000 ALTER TABLE `mall_sitesetting` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_sitesigninconfig`
--

DROP TABLE IF EXISTS `mall_sitesigninconfig`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_sitesigninconfig` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `IsEnable` tinyint(1) NOT NULL COMMENT '开启签到',
  `DayIntegral` int(11) NOT NULL DEFAULT '0' COMMENT '签到获得积分',
  `DurationCycle` int(11) NOT NULL DEFAULT '0' COMMENT '持续周期',
  `DurationReward` int(11) NOT NULL DEFAULT '0' COMMENT '周期额外奖励积分',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_sitesigninconfig`
--

LOCK TABLES `mall_sitesigninconfig` WRITE;
/*!40000 ALTER TABLE `mall_sitesigninconfig` DISABLE KEYS */;
INSERT INTO `mall_sitesigninconfig` VALUES (2,1,10,5,20);
/*!40000 ALTER TABLE `mall_sitesigninconfig` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_sku`
--

DROP TABLE IF EXISTS `mall_sku`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_sku` (
  `Id` varchar(100) NOT NULL COMMENT '商品ID_颜色规格ID_颜色规格ID_尺寸规格',
  `AutoId` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '自增主键Id',
  `ProductId` bigint(20) NOT NULL COMMENT '商品ID',
  `Color` varchar(100) DEFAULT NULL COMMENT '颜色规格',
  `Size` varchar(100) DEFAULT NULL COMMENT '尺寸规格',
  `Version` varchar(100) DEFAULT NULL COMMENT '版本规格',
  `Sku` varchar(100) DEFAULT NULL COMMENT 'SKU',
  `Stock` bigint(20) NOT NULL COMMENT '库存',
  `CostPrice` decimal(18,2) NOT NULL COMMENT '成本价',
  `SalePrice` decimal(18,2) NOT NULL COMMENT '销售价',
  `ShowPic` varchar(200) DEFAULT NULL COMMENT '显示图片',
  `SafeStock` bigint(20) NOT NULL DEFAULT '0' COMMENT '警戒库存',
  PRIMARY KEY (`AutoId`) USING BTREE,
  KEY `FK_Product_Sku` (`ProductId`) USING BTREE,
  KEY `AutoId` (`AutoId`) USING BTREE,
  KEY `Id` (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=7699 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_sku`
--

LOCK TABLES `mall_sku` WRITE;
/*!40000 ALTER TABLE `mall_sku` DISABLE KEYS */;
INSERT INTO `mall_sku` VALUES ('702_0_0_0',5036,702,NULL,NULL,NULL,NULL,1998,0.00,11.90,NULL,0),('704_0_0_0',5038,704,NULL,NULL,NULL,NULL,2000,0.00,499.00,NULL,0),('705_0_0_0',5039,705,NULL,NULL,NULL,NULL,2000,0.00,399.00,NULL,0),('706_0_0_0',5040,706,NULL,NULL,NULL,NULL,1000,0.00,234.00,NULL,0),('709_0_0_0',5044,709,NULL,NULL,NULL,NULL,2000,0.00,599.00,NULL,0),('707_655_663_0',5046,707,'黑色','175/92(L)',NULL,NULL,4996,0.00,175.00,'/Storage/Shop/1/Products/707/skus/707_655_663_0.png',0),('703_0_0_0',5047,703,NULL,NULL,NULL,NULL,1998,0.00,10.80,NULL,0),('701_0_0_0',5048,701,NULL,NULL,NULL,NULL,1999,0.00,29.90,NULL,0),('710_0_0_0',5050,710,NULL,NULL,NULL,NULL,2000,0.00,250.00,NULL,0),('699_0_0_0',5051,699,NULL,NULL,NULL,NULL,999,0.00,49.90,NULL,0),('700_0_0_0',5052,700,NULL,NULL,NULL,NULL,1999,0.00,59.90,NULL,0),('711_0_0_0',5053,711,NULL,NULL,NULL,NULL,2000,0.00,169.00,NULL,0),('712_0_661_0',5054,712,NULL,'165/84(S)',NULL,NULL,1000,0.00,229.00,NULL,0),('712_0_659_0',5055,712,NULL,'160/80(XS)',NULL,NULL,1000,0.00,229.00,NULL,0),('718_0_0_0',5062,718,NULL,NULL,NULL,NULL,2000,0.00,110.00,NULL,0),('717_0_0_0',5063,717,NULL,NULL,NULL,NULL,20,0.00,19.90,NULL,20),('716_0_0_0',5064,716,NULL,NULL,NULL,NULL,2000,0.00,158.00,NULL,20),('715_0_0_0',5065,715,NULL,NULL,NULL,NULL,10000,0.00,13688.00,NULL,0),('714_0_0_0',5066,714,NULL,NULL,NULL,NULL,2000,0.00,6988.00,NULL,0),('713_0_0_0',5067,713,NULL,NULL,NULL,NULL,20,0.00,50.00,NULL,20),('708_0_0_0',5068,708,NULL,NULL,NULL,NULL,31,0.00,450.00,NULL,30),('719_0_0_0',5069,719,NULL,NULL,NULL,NULL,500,0.00,37.00,NULL,0),('720_0_0_0',5070,720,NULL,NULL,NULL,NULL,5000,0.00,32.90,NULL,0),('721_0_0_0',5071,721,NULL,NULL,NULL,NULL,5000,0.00,55.00,NULL,0),('722_0_0_0',5072,722,NULL,NULL,NULL,NULL,5000,0.00,110.00,NULL,0),('723_0_0_0',5073,723,NULL,NULL,NULL,NULL,5000,0.00,50.00,NULL,0),('724_0_0_0',5075,724,NULL,NULL,NULL,NULL,5000,0.00,248.00,NULL,0),('725_0_0_0',5076,725,NULL,NULL,NULL,NULL,5000,0.00,248.00,NULL,0),('726_0_0_0',5077,726,NULL,NULL,NULL,NULL,5000,0.00,388.00,NULL,0),('727_0_0_0',5078,727,NULL,NULL,NULL,NULL,5000,0.00,98.00,NULL,0),('728_0_0_0',5079,728,NULL,NULL,NULL,NULL,5000,0.00,189.00,NULL,0),('729_0_0_0',5080,729,NULL,NULL,NULL,NULL,5000,0.00,189.00,NULL,0),('730_0_0_0',5081,730,NULL,NULL,NULL,NULL,5000,0.00,189.00,NULL,0),('733_0_0_0',5084,733,NULL,NULL,NULL,NULL,999,0.00,4425.00,NULL,0),('734_0_0_0',5085,734,NULL,NULL,NULL,NULL,1000,0.00,63.00,NULL,0),('735_0_0_0',5086,735,NULL,NULL,NULL,NULL,800,0.00,98.00,NULL,0),('737_0_0_0',5088,737,NULL,NULL,NULL,NULL,25,0.00,83.00,NULL,10),('738_0_0_0',5089,738,NULL,NULL,NULL,NULL,5000,0.00,35.00,NULL,0),('739_0_0_0',5090,739,NULL,NULL,NULL,NULL,5000,0.00,4499.00,NULL,0),('741_0_0_0',5092,741,NULL,NULL,NULL,NULL,2000,0.00,5399.00,NULL,0),('736_0_0_0',5093,736,NULL,NULL,NULL,NULL,5000,0.00,33.00,NULL,0),('743_669_0_0',5095,743,'银色',NULL,NULL,NULL,20,0.00,2999.00,NULL,10),('743_671_0_0',5096,743,'白色',NULL,NULL,NULL,20,0.00,2999.00,NULL,10),('743_670_0_0',5097,743,'黑色',NULL,NULL,NULL,20,0.00,2999.00,NULL,10),('744_0_0_0',5098,744,NULL,NULL,NULL,NULL,20,0.00,469.00,NULL,10),('745_0_0_0',5099,745,NULL,NULL,NULL,NULL,10,0.00,219.00,NULL,1),('731_675_0_0',5113,731,'蓝色',NULL,NULL,'123123',10000,0.00,99.00,'/Storage/Shop/1/Products/731/skus/731_675_0_0.png',10),('731_677_0_0',5114,731,'白色',NULL,NULL,'345345',10000,0.00,99.00,'/Storage/Shop/1/Products/731/skus/731_677_0_0.png',10),('731_674_0_0',5115,731,'绿色',NULL,NULL,'678678',10000,0.00,99.00,'/Storage/Shop/1/Products/731/skus/731_674_0_0.png',10),('731_676_0_0',5116,731,'黄色',NULL,NULL,'890890',10000,0.00,99.00,'/Storage/Shop/1/Products/731/skus/731_676_0_0.png',10),('731_673_0_0',5117,731,'红色',NULL,NULL,'098098',10000,0.00,99.00,'/Storage/Shop/1/Products/731/skus/731_673_0_0.png',10),('731_678_0_0',5118,731,'黑色',NULL,NULL,'789789',10000,0.00,99.00,'/Storage/Shop/1/Products/731/skus/731_678_0_0.png',10),('732_0_679_0',5119,732,NULL,'1.4M书桌',NULL,'898989',6000,0.00,200.00,NULL,0),('732_0_681_0',5120,732,NULL,'1.8M书桌',NULL,'895623',6000,0.00,200.00,NULL,0),('732_0_680_0',5121,732,NULL,'1.2M书桌',NULL,'121212',6000,0.00,200.00,NULL,0),('746_0_0_0',5122,746,NULL,NULL,NULL,NULL,8000,0.00,28.00,NULL,0),('747_0_0_0',5123,747,NULL,NULL,NULL,NULL,9000,0.00,1000.00,NULL,0),('748_0_0_0',5124,748,NULL,NULL,NULL,NULL,5000,0.00,249.00,NULL,0),('749_0_0_0',5125,749,NULL,NULL,NULL,NULL,600,0.00,20000.00,NULL,0),('740_0_0_0',5126,740,NULL,NULL,NULL,NULL,20,0.00,25.00,NULL,10),('742_0_0_0',5127,742,NULL,NULL,NULL,NULL,20,0.00,168.00,NULL,50);
/*!40000 ALTER TABLE `mall_sku` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_slidead`
--

DROP TABLE IF EXISTS `mall_slidead`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_slidead` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID，0：平台轮播图  ',
  `ImageUrl` varchar(100) NOT NULL COMMENT '图片保存URL',
  `Url` varchar(1000) NOT NULL COMMENT '图片跳转URL',
  `DisplaySequence` bigint(20) NOT NULL,
  `TypeId` int(11) NOT NULL DEFAULT '0',
  `Description` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=312 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_slidead`
--

LOCK TABLES `mall_slidead` WRITE;
/*!40000 ALTER TABLE `mall_slidead` DISABLE KEYS */;
INSERT INTO `mall_slidead` VALUES (105,0,'/Storage/Plat/ImageAd/201702141456215116690.jpg','./product/detail/723',1,1,NULL),(106,0,'/Storage/Plat/ImageAd/201702141456275819570.jpg','./product/detail/723',2,1,NULL),(107,0,'/Storage/Plat/ImageAd/201702141126285292890.jpg','./LimitTimeBuy/Detail/45',3,1,NULL),(108,0,'/Storage/Plat/ImageAd/201702141456377958540.jpg','./product/detail/734',4,1,NULL),(109,0,'/Storage/Plat/ImageAd/201702141456476728430.jpg','./LimitTimeBuy/Detail/45',5,1,NULL),(110,0,'/Storage/Plat/ImageAd/201702141507054403840.jpg','/',6,2,NULL),(111,0,'/Storage/Plat/ImageAd/201702141507119013210.jpg','/',7,2,NULL),(112,0,'/Storage/Plat/APP/SlidAd/201702141656064131340.png','./product/detail/735',8,8,''),(113,0,'/Storage/Plat/APP/SlidAd/201702141656402812130.jpg','./product/detail/745',9,8,''),(114,0,'/Storage/Plat/APP/SlidAd/201702141729484199440.jpg','./product/detail/713',10,8,''),(115,0,'/temp/201702151053059693630.png','5',11,10,'分类'),(116,0,'/temp/201702151053171172520.png','3',12,10,'限时购'),(117,0,'/temp/201702151053291655660.png','2',13,10,'拼团'),(118,0,'/temp/201702151053418652720.png','1',15,10,'专题'),(119,1,'/Storage/Shop/1/VShop/201702141740119921260.png','./product/detail/734',1,4,NULL),(120,1,'/Storage/Shop/1/VShop/201702141740317343450.png','./LimitTimeBuy/Detail/43',2,4,NULL),(121,0,'/temp/201702151053500286720.png','4',14,10,'积分商城'),(122,0,'/Storage/Plat/APP/SlidAd/201702151042004701760.jpg','./limittimebuy/detail/50',16,9,''),(123,0,'/Storage/Plat/APP/SlidAd/201702151042325485560.jpg','./product/detail/738',17,9,''),(124,0,'/Storage/Plat/APP/SlidAd/201702151042594568160.jpg','./product/detail/723',18,9,''),(125,0,'/temp/201702161535154633570.jpg','https://www.baidu.com/',19,11,'1'),(126,0,'/temp/201702161535498181920.jpg','https://www.baidu.com/',20,11,'2'),(127,1,'/Storage/Shop/1/ImageAd/201702161745280922140.jpg','/',3,3,NULL),(128,1,'/Storage/Shop/1/ImageAd/201702161745417915320.jpg','/',4,3,NULL),(131,1,'/Storage/Shop/1/ImageAd/201702161745488574860.jpg','/',5,3,NULL),(132,1,'/Storage/Shop/1/ImageAd/201702161746002073460.jpg','/',6,3,NULL);
/*!40000 ALTER TABLE `mall_slidead` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_specificationvalue`
--

DROP TABLE IF EXISTS `mall_specificationvalue`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_specificationvalue` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Specification` int(11) NOT NULL COMMENT '规格名',
  `TypeId` bigint(20) NOT NULL COMMENT '类型ID',
  `Value` varchar(100) NOT NULL COMMENT '规格值',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Type_SpecificationValue` (`TypeId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=1150 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_specificationvalue`
--

LOCK TABLES `mall_specificationvalue` WRITE;
/*!40000 ALTER TABLE `mall_specificationvalue` DISABLE KEYS */;
INSERT INTO `mall_specificationvalue` VALUES (646,1,83,'紫色'),(647,1,83,'红色'),(648,1,83,'绿色'),(649,1,83,'花色'),(650,1,83,'蓝色'),(651,1,83,'褐色'),(652,1,83,'透明'),(653,1,83,'酒红色'),(654,1,83,'黄色'),(655,1,83,'黑色'),(656,1,83,'深灰色'),(657,1,83,'深紫色'),(658,1,83,'深蓝色'),(659,2,83,'160/80(XS)'),(660,2,83,'190/110(XXXL)'),(661,2,83,'165/84(S)'),(662,2,83,'170/88(M)'),(663,2,83,'175/92(L)'),(664,2,83,'180/96(XL)'),(665,2,83,'185/100(XXL)'),(666,2,83,'160/84(XS)'),(667,2,83,'165/88(S)'),(668,2,83,'170/92(M)'),(669,1,84,'银色'),(670,1,84,'黑色'),(671,1,84,'白色'),(672,1,88,'紫色'),(673,1,88,'红色'),(674,1,88,'绿色'),(675,1,88,'蓝色'),(676,1,88,'黄色'),(677,1,88,'白色'),(678,1,88,'黑色'),(679,2,88,'1.4M书桌'),(680,2,88,'1.2M书桌'),(681,2,88,'1.8M书桌');
/*!40000 ALTER TABLE `mall_specificationvalue` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_statisticordercomment`
--

DROP TABLE IF EXISTS `mall_statisticordercomment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_statisticordercomment` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL,
  `CommentKey` int(11) NOT NULL COMMENT '评价的枚举（宝贝与描述相符 商家得分）',
  `CommentValue` decimal(10,4) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `mall_statisticordercomments_ibfk_1` (`ShopId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=gbk;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_statisticordercomment`
--

LOCK TABLES `mall_statisticordercomment` WRITE;
/*!40000 ALTER TABLE `mall_statisticordercomment` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_statisticordercomment` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_templatevisualizationsetting`
--

DROP TABLE IF EXISTS `mall_templatevisualizationsetting`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_templatevisualizationsetting` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `CurrentTemplateName` varchar(2000) NOT NULL COMMENT '当前使用的模板的名称',
  `ShopId` bigint(20) NOT NULL COMMENT '店铺Id（平台为0）',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_templatevisualizationsetting`
--

LOCK TABLES `mall_templatevisualizationsetting` WRITE;
/*!40000 ALTER TABLE `mall_templatevisualizationsetting` DISABLE KEYS */;
INSERT INTO `mall_templatevisualizationsetting` VALUES (3,'t9',0);
/*!40000 ALTER TABLE `mall_templatevisualizationsetting` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_theme`
--

DROP TABLE IF EXISTS `mall_theme`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_theme` (
  `ThemeId` bigint(20) NOT NULL AUTO_INCREMENT,
  `TypeId` int(11) NOT NULL COMMENT '0、默认主题；1、自定义主题',
  `MainColor` varchar(50) DEFAULT NULL COMMENT '商城主色',
  `SecondaryColor` varchar(50) DEFAULT NULL COMMENT '商城辅色',
  `WritingColor` varchar(50) DEFAULT NULL COMMENT '字体颜色',
  `FrameColor` varchar(50) DEFAULT NULL COMMENT '边框颜色',
  `ClassifiedsColor` varchar(50) DEFAULT NULL COMMENT '商品分类栏',
  `IsUse` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否将主题配色应用至商城',
  PRIMARY KEY (`ThemeId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='主题设置表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_theme`
--

LOCK TABLES `mall_theme` WRITE;
/*!40000 ALTER TABLE `mall_theme` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_theme` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_topic`
--

DROP TABLE IF EXISTS `mall_topic`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_topic` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL COMMENT '专题名称',
  `FrontCoverImage` varchar(100) DEFAULT NULL COMMENT '封面图片',
  `TopImage` varchar(100) NOT NULL COMMENT '主图',
  `BackgroundImage` varchar(100) DEFAULT NULL COMMENT '背景图片',
  `PlatForm` int(11) NOT NULL DEFAULT '0' COMMENT '使用终端',
  `Tags` varchar(100) DEFAULT NULL COMMENT '标签',
  `ShopId` bigint(20) NOT NULL DEFAULT '0' COMMENT '店铺ID',
  `IsRecommend` tinyint(1) unsigned zerofill NOT NULL COMMENT '是否推荐',
  `SelfDefineText` text COMMENT '自定义热点',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=138 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_topic`
--

LOCK TABLES `mall_topic` WRITE;
/*!40000 ALTER TABLE `mall_topic` DISABLE KEYS */;
INSERT INTO `mall_topic` VALUES (54,'情人节有礼','','/Storage/Plat/Topic/54/201702141026309101440.jpg','',99,'爱情',0,0,NULL),(55,'情人节钜惠活动',NULL,'/Storage/Plat/Topic/55/201702141036098564450.jpg','/Storage/Plat/Topic/55/201702141031428398320.jpg',0,NULL,0,1,''),(56,'情人节大放送','','/Storage/Plat/Topic/56/201702141108011679570.jpg','',99,'爱情',1,0,NULL),(57,'2月14好礼相送','','/Storage/Plat/Topic/57/201702141112100019070.jpg','',99,'情人节',1,0,NULL),(58,'214你约会我优惠','','/Storage/Plat/Topic/58/201702141117396835540.jpg','',99,'214',1,0,NULL),(60,'家居综合',NULL,'/Storage/Plat/Topic/60/201702161641282371640.jpg','/Storage/Plat/Topic/60/201702161641341465710.png',0,NULL,0,1,''),(61,'端午节大酬宾',NULL,'/Storage/Plat/Topic/61/201702161444494454990.jpg','/Storage/Plat/Topic/61/201702161631467748550.png',0,NULL,0,1,''),(62,'214你约会我优惠','','/Storage/Plat/Topic/62/201702161459410994070.jpg','',99,'爱情',0,0,NULL),(63,'百货','','/Storage/Plat/Topic/63/201702161501044258490.jpg','',99,'优惠',0,0,NULL);
/*!40000 ALTER TABLE `mall_topic` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_topicmodule`
--

DROP TABLE IF EXISTS `mall_topicmodule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_topicmodule` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `TopicId` bigint(20) NOT NULL COMMENT '专题ID',
  `Name` varchar(100) NOT NULL COMMENT '专题名称',
  `TitleAlign` int(11) NOT NULL COMMENT '标题位置 0、left；1、center ；2、right',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Topic_TopicModule` (`TopicId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=276 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_topicmodule`
--

LOCK TABLES `mall_topicmodule` WRITE;
/*!40000 ALTER TABLE `mall_topicmodule` DISABLE KEYS */;
INSERT INTO `mall_topicmodule` VALUES (213,57,'服装',0),(214,57,'默认模块2',0),(215,57,'默认模块3',0),(216,56,'衣服',0),(217,56,'食品',0),(218,56,'家居',0),(219,58,'默认模块1',0),(220,58,'默认模块2',0),(221,58,'默认模块3',0),(245,54,'时尚潮流',0),(246,54,'休闲零食',0),(250,61,'食品',0),(251,61,'服装',0),(252,61,'家居',0),(260,60,'家居',0),(264,55,'零食',0),(265,55,'服装',0),(266,55,'家居',0),(267,63,'食品',0),(268,63,'服装',0),(269,63,'家居',0),(273,62,'家居',0),(274,62,'食品',0),(275,62,'清洁',0);
/*!40000 ALTER TABLE `mall_topicmodule` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_type`
--

DROP TABLE IF EXISTS `mall_type`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_type` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL COMMENT '类型名称',
  `IsSupportColor` tinyint(1) NOT NULL COMMENT '是否支持颜色规格',
  `IsSupportSize` tinyint(1) NOT NULL COMMENT '是否支持尺寸规格',
  `IsSupportVersion` tinyint(1) NOT NULL COMMENT '是否支持版本规格',
  `IsDeleted` bit(1) NOT NULL COMMENT '是否已删除',
  `ColorAlias` varchar(50) DEFAULT NULL COMMENT '颜色别名',
  `SizeAlias` varchar(50) DEFAULT NULL COMMENT '尺码别名',
  `VersionAlias` varchar(50) DEFAULT NULL COMMENT '规格别名',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=99 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_type`
--

LOCK TABLES `mall_type` WRITE;
/*!40000 ALTER TABLE `mall_type` DISABLE KEYS */;
INSERT INTO `mall_type` VALUES (82,'零食',0,0,0,'\0',NULL,NULL,NULL),(83,'衣服',1,1,0,'\0',NULL,NULL,NULL),(84,'电脑办公',1,0,0,'\0',NULL,NULL,NULL),(85,'生鲜',0,0,0,'\0',NULL,NULL,NULL),(86,'家用电器',0,0,0,'\0',NULL,NULL,NULL),(87,'化妆',0,0,0,'\0',NULL,NULL,NULL),(88,'家居',1,1,0,'\0',NULL,NULL,NULL),(89,'鞋包',0,0,0,'\0',NULL,NULL,NULL),(90,'运动',0,0,0,'\0',NULL,NULL,NULL),(91,'汽车用品',0,0,0,'\0',NULL,NULL,NULL),(92,'母婴',0,0,0,'\0',NULL,NULL,NULL),(93,'医药保健',0,0,0,'\0',NULL,NULL,NULL),(94,'图书',0,0,0,'\0',NULL,NULL,NULL),(95,'钟表',0,0,0,'\0',NULL,NULL,NULL),(96,'珠宝',0,0,0,'\0',NULL,NULL,NULL);
/*!40000 ALTER TABLE `mall_type` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_typebrand`
--

DROP TABLE IF EXISTS `mall_typebrand`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_typebrand` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `TypeId` bigint(20) NOT NULL,
  `BrandId` bigint(20) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_Brand_TypeBrand` (`BrandId`) USING BTREE,
  KEY `FK_Type_TypeBrand` (`TypeId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2049 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_typebrand`
--

LOCK TABLES `mall_typebrand` WRITE;
/*!40000 ALTER TABLE `mall_typebrand` DISABLE KEYS */;
INSERT INTO `mall_typebrand` VALUES (1720,82,319),(1721,82,320),(1722,83,321),(1724,85,323),(1726,87,324),(1727,87,328),(1728,84,322),(1729,93,350),(1730,93,348);
/*!40000 ALTER TABLE `mall_typebrand` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_verificationrecord`
--

DROP TABLE IF EXISTS `mall_verificationrecord`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_verificationrecord` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `OrderId` bigint(20) NOT NULL COMMENT '订单ID',
  `VerificationCodeIds` varchar(1000) NOT NULL COMMENT '核销码ID集合',
  `VerificationTime` datetime NOT NULL COMMENT '核销时间',
  `VerificationUser` varchar(50) NOT NULL COMMENT '核销人',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='虚拟订单核销记录表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_verificationrecord`
--

LOCK TABLES `mall_verificationrecord` WRITE;
/*!40000 ALTER TABLE `mall_verificationrecord` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_verificationrecord` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_virtualorderitem`
--

DROP TABLE IF EXISTS `mall_virtualorderitem`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_virtualorderitem` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `OrderId` bigint(20) NOT NULL COMMENT '订单ID',
  `OrderItemId` bigint(20) NOT NULL COMMENT '订单项ID',
  `VirtualProductItemName` varchar(25) NOT NULL COMMENT '虚拟商品信息项名称',
  `Content` varchar(1000) DEFAULT NULL COMMENT '信息项填写内容',
  `VirtualProductItemType` tinyint(4) NOT NULL COMMENT '信息项类型(1=文本格式，2=日期，3=时间，4=身份证，5=数字格式，6=图片)',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=297 DEFAULT CHARSET=utf8 COMMENT='虚拟订单信息项表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_virtualorderitem`
--

LOCK TABLES `mall_virtualorderitem` WRITE;
/*!40000 ALTER TABLE `mall_virtualorderitem` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_virtualorderitem` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_virtualproduct`
--

DROP TABLE IF EXISTS `mall_virtualproduct`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_virtualproduct` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ProductId` bigint(20) NOT NULL COMMENT '商品ID',
  `ValidityType` bit(1) NOT NULL COMMENT '商品有效期类型(0=长期有效，1=自定义日期)',
  `StartDate` datetime DEFAULT NULL COMMENT '自定义开始时间',
  `EndDate` datetime DEFAULT NULL COMMENT '自定义结束时间',
  `EffectiveType` tinyint(4) NOT NULL COMMENT '核销码生效类型（1=立即生效，2=付款完成X小时后生效，3=次日生效）',
  `Hour` int(11) NOT NULL COMMENT '付款完成X小时后生效',
  `SupportRefundType` tinyint(4) NOT NULL COMMENT '1=支持有效期内退款，2=支持随时退款，3=不支持退款',
  `UseNotice` varchar(400) DEFAULT '' COMMENT '使用须知',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=52 DEFAULT CHARSET=utf8 COMMENT='虚拟商品表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_virtualproduct`
--

LOCK TABLES `mall_virtualproduct` WRITE;
/*!40000 ALTER TABLE `mall_virtualproduct` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_virtualproduct` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_virtualproductitem`
--

DROP TABLE IF EXISTS `mall_virtualproductitem`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_virtualproductitem` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `ProductId` bigint(20) NOT NULL COMMENT '商品ID',
  `Name` varchar(25) NOT NULL COMMENT '信息项标题名称',
  `Type` tinyint(4) NOT NULL COMMENT '信息项类型(1=文本格式，2=日期，3=时间，4=身份证，5=数字格式，6=图片)',
  `Required` bit(1) NOT NULL COMMENT '是否必填(0=否，1=是)',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=240 DEFAULT CHARSET=utf8 COMMENT='虚拟商品信息项表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_virtualproductitem`
--

LOCK TABLES `mall_virtualproductitem` WRITE;
/*!40000 ALTER TABLE `mall_virtualproductitem` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_virtualproductitem` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_vshop`
--

DROP TABLE IF EXISTS `mall_vshop`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_vshop` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Name` varchar(20) DEFAULT NULL COMMENT '名称',
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `CreateTime` datetime NOT NULL COMMENT '创建日期',
  `VisitNum` int(11) NOT NULL COMMENT '历览次数',
  `buyNum` int(11) NOT NULL COMMENT '购买数量',
  `State` int(11) NOT NULL COMMENT '状态',
  `Logo` varchar(200) DEFAULT NULL COMMENT 'LOGO',
  `BackgroundImage` varchar(200) DEFAULT NULL COMMENT '背景图',
  `Description` varchar(30) DEFAULT NULL COMMENT '详情',
  `Tags` varchar(100) DEFAULT NULL COMMENT '标签',
  `HomePageTitle` varchar(20) DEFAULT NULL COMMENT '微信首页显示的标题',
  `WXLogo` varchar(200) DEFAULT NULL COMMENT '微信Logo',
  `IsOpen` tinyint(1) NOT NULL DEFAULT '1' COMMENT '是否开启微店',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_vshop_shopinfo` (`ShopId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_vshop`
--

LOCK TABLES `mall_vshop` WRITE;
/*!40000 ALTER TABLE `mall_vshop` DISABLE KEYS */;
INSERT INTO `mall_vshop` VALUES (10,'官方自营店',1,'2017-02-14 17:37:32',13,16,2,'/Storage/Shop/1/VShop/201702141737310419930.png','/Storage/Shop/1/VShop/201702141734403329540.png',NULL,'官方自营',NULL,'/Storage/Shop/1/VShop/201702141734365302700.png',1);
/*!40000 ALTER TABLE `mall_vshop` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_vshopextend`
--

DROP TABLE IF EXISTS `mall_vshopextend`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_vshopextend` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `VShopId` bigint(20) NOT NULL COMMENT '微店ID',
  `Sequence` int(11) NOT NULL COMMENT '顺序',
  `Type` int(11) NOT NULL COMMENT '微店类型（主推微店、热门微店）',
  `AddTime` datetime NOT NULL COMMENT '添加时间',
  `State` int(11) NOT NULL COMMENT '审核状态',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_vshopextend_vshop` (`VShopId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=30 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_vshopextend`
--

LOCK TABLES `mall_vshopextend` WRITE;
/*!40000 ALTER TABLE `mall_vshopextend` DISABLE KEYS */;
INSERT INTO `mall_vshopextend` VALUES (29,10,1,1,'2017-02-16 15:26:00',0);
/*!40000 ALTER TABLE `mall_vshopextend` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_weiactivityaward`
--

DROP TABLE IF EXISTS `mall_weiactivityaward`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_weiactivityaward` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ActivityId` bigint(20) NOT NULL,
  `AwardLevel` int(11) NOT NULL COMMENT '保存字段1-10 分别对应1至10等奖',
  `AwardType` int(11) NOT NULL COMMENT '积分；红包；优惠卷',
  `AwardCount` int(11) NOT NULL,
  `Proportion` float NOT NULL,
  `Integral` int(11) NOT NULL,
  `BonusId` bigint(20) NOT NULL DEFAULT '0',
  `CouponId` bigint(20) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_mall_WeiActivityAward_2` (`ActivityId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=369 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_weiactivityaward`
--

LOCK TABLES `mall_weiactivityaward` WRITE;
/*!40000 ALTER TABLE `mall_weiactivityaward` DISABLE KEYS */;
INSERT INTO `mall_weiactivityaward` VALUES (305,154,1,0,5,1,520,0,0),(306,154,2,0,15,5,200,0,0),(307,154,3,0,50,6,100,0,0),(308,155,1,0,50,2,520,0,0),(309,155,2,0,100,4,200,0,0),(310,155,3,0,200,5,100,0,0);
/*!40000 ALTER TABLE `mall_weiactivityaward` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_weiactivityinfo`
--

DROP TABLE IF EXISTS `mall_weiactivityinfo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_weiactivityinfo` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ActivityTitle` varchar(200) NOT NULL,
  `ActivityType` int(11) NOT NULL,
  `ActivityDetails` varchar(500) NOT NULL,
  `ActivityUrl` varchar(300) NOT NULL,
  `BeginTime` datetime NOT NULL,
  `EndTime` datetime NOT NULL,
  `ParticipationType` int(11) NOT NULL COMMENT '0 共几次 1天几次 2无限制',
  `ParticipationCount` int(11) NOT NULL,
  `ConsumePoint` int(11) NOT NULL COMMENT '0不消耗积分 大于0消耗积分',
  `CodeUrl` varchar(300) DEFAULT NULL,
  `AddDate` datetime NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=180 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_weiactivityinfo`
--

LOCK TABLES `mall_weiactivityinfo` WRITE;
/*!40000 ALTER TABLE `mall_weiactivityinfo` DISABLE KEYS */;
INSERT INTO `mall_weiactivityinfo` VALUES (154,'情人节刮一刮',0,'情人节刮一刮，转出爱情，转出鲜花','/Storage/Plat/WeiActivity/WeiActivity_20170214104941480463.jpg','2017-02-14 11:40:09','2017-03-30 11:40:30',1,2,5,'/Storage/Plat/WeiActivity/321cd0da-dba6-4cac-ba85-465ea8a69da9.jpg','2017-02-14 10:49:41'),(155,'情人节转一转',1,'情人节转一转，转出鲜花，转出爱情','/Storage/Plat/WeiActivity/WeiActivity_20170214105428946248.jpg','2017-02-14 12:50:43','2017-05-01 10:50:43',1,2,10,'/Storage/Plat/WeiActivity/42b250c2-1a5b-4744-87a1-08ca6a3af659.jpg','2017-02-14 10:54:29');
/*!40000 ALTER TABLE `mall_weiactivityinfo` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_weiactivitywininfo`
--

DROP TABLE IF EXISTS `mall_weiactivitywininfo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_weiactivitywininfo` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `UserId` bigint(20) NOT NULL,
  `ActivityId` bigint(20) NOT NULL,
  `AwardId` bigint(20) NOT NULL,
  `IsWin` tinyint(1) NOT NULL,
  `AwardName` varchar(200) DEFAULT NULL,
  `AddDate` datetime NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_mall_WeiActivityWinInfo_W2` (`ActivityId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_weiactivitywininfo`
--

LOCK TABLES `mall_weiactivitywininfo` WRITE;
/*!40000 ALTER TABLE `mall_weiactivitywininfo` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_weiactivitywininfo` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_weixinbasic`
--

DROP TABLE IF EXISTS `mall_weixinbasic`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_weixinbasic` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Ticket` varchar(200) DEFAULT NULL COMMENT '微信Ticket',
  `TicketOutTime` datetime NOT NULL COMMENT '微信Ticket过期日期',
  `AppId` varchar(50) DEFAULT NULL,
  `AccessToken` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_weixinbasic`
--

LOCK TABLES `mall_weixinbasic` WRITE;
/*!40000 ALTER TABLE `mall_weixinbasic` DISABLE KEYS */;
INSERT INTO `mall_weixinbasic` VALUES (3,'bxLdikRXVbTPdHSM05e5u3TL3kJRrCdIn85vLIHAL0CUL86UY846d_wuJkIUsk_XkDfhBopLdzOOR2sDpJMZZg','2017-02-20 15:20:08','wx007e3e45af8e55dd','5Tw9O90rhujPEn_OUhIA8fBfr644_hiB6HznXhfC7mDGlhmTYo6zAEHCmSBDJMzLcY98GWxGyqqO6tDxzZb1BvEwHqrDcIYLeIUQmhNQXfK1oxmdHlr_QtM7qiBgOKfzFCLlCIALIR');
/*!40000 ALTER TABLE `mall_weixinbasic` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_weixinmsgtemplate`
--

DROP TABLE IF EXISTS `mall_weixinmsgtemplate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_weixinmsgtemplate` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `MessageType` int(11) NOT NULL COMMENT '消息类别',
  `TemplateNum` varchar(30) DEFAULT NULL COMMENT '消息模板编号',
  `TemplateId` varchar(100) DEFAULT NULL COMMENT '消息模板ID',
  `UpdateDate` datetime NOT NULL COMMENT '更新日期',
  `IsOpen` tinyint(1) NOT NULL COMMENT '是否启用',
  `UserInWxApplet` tinyint(4) unsigned zerofill NOT NULL DEFAULT '0000' COMMENT '是否小程序微信通知',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=162 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_weixinmsgtemplate`
--

LOCK TABLES `mall_weixinmsgtemplate` WRITE;
/*!40000 ALTER TABLE `mall_weixinmsgtemplate` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_weixinmsgtemplate` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_wxacctoken`
--

DROP TABLE IF EXISTS `mall_wxacctoken`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_wxacctoken` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `AppId` varchar(50) DEFAULT NULL,
  `AccessToken` varchar(150) NOT NULL COMMENT '微信访问令牌',
  `TokenOutTime` datetime NOT NULL COMMENT '微信令牌过期日期',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_wxacctoken`
--

LOCK TABLES `mall_wxacctoken` WRITE;
/*!40000 ALTER TABLE `mall_wxacctoken` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_wxacctoken` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_wxappletformdata`
--

DROP TABLE IF EXISTS `mall_wxappletformdata`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_wxappletformdata` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `EventId` bigint(20) NOT NULL COMMENT '事件ID',
  `EventValue` varchar(255) DEFAULT NULL COMMENT '事件值',
  `FormId` varchar(255) DEFAULT NULL COMMENT '事件的表单ID',
  `EventTime` datetime NOT NULL COMMENT '事件时间',
  `ExpireTime` datetime NOT NULL COMMENT 'FormId过期时间',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_wxappletformdata`
--

LOCK TABLES `mall_wxappletformdata` WRITE;
/*!40000 ALTER TABLE `mall_wxappletformdata` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_wxappletformdata` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_wxcardcodelog`
--

DROP TABLE IF EXISTS `mall_wxcardcodelog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_wxcardcodelog` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `CardLogId` bigint(20) NOT NULL COMMENT '卡券记录号',
  `CardId` varchar(50) DEFAULT NULL,
  `Code` varchar(50) DEFAULT NULL COMMENT '标识',
  `SendTime` datetime NOT NULL COMMENT '投放时间',
  `CodeStatus` int(11) NOT NULL DEFAULT '0' COMMENT '状态',
  `UsedTime` datetime DEFAULT NULL COMMENT '操作时间 失效、核销、删除时间',
  `CouponType` int(11) NOT NULL COMMENT '红包类型',
  `CouponCodeId` bigint(20) NOT NULL COMMENT '红包记录编号',
  `OpenId` varchar(4000) DEFAULT NULL COMMENT '对应OpenId',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `FK_mall_WXLog_CardLogId` (`CardLogId`) USING BTREE,
  KEY `IDX_mall_WXLog_CardId` (`CardId`) USING BTREE,
  KEY `IDX_mall_WXLog_Code` (`Code`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_wxcardcodelog`
--

LOCK TABLES `mall_wxcardcodelog` WRITE;
/*!40000 ALTER TABLE `mall_wxcardcodelog` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_wxcardcodelog` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_wxcardlog`
--

DROP TABLE IF EXISTS `mall_wxcardlog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_wxcardlog` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '编号',
  `CardId` varchar(50) DEFAULT NULL COMMENT '卡券编号',
  `CardTitle` varchar(50) DEFAULT NULL COMMENT '标题 英文27  汉字 9个',
  `CardSubTitle` varchar(100) DEFAULT NULL COMMENT '副标题 英文54  汉字18个',
  `CardColor` varchar(10) DEFAULT NULL COMMENT '卡券颜色 HasTable',
  `AuditStatus` int(11) NOT NULL DEFAULT '0' COMMENT '审核状态',
  `AppId` varchar(50) DEFAULT NULL,
  `AppSecret` varchar(50) DEFAULT NULL,
  `CouponType` int(11) NOT NULL COMMENT '红包类型',
  `CouponId` bigint(20) NOT NULL COMMENT '红包编号 涉及多表，不做外键',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `IDX_mall_WXCardLog_CardId` (`CardId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_wxcardlog`
--

LOCK TABLES `mall_wxcardlog` WRITE;
/*!40000 ALTER TABLE `mall_wxcardlog` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_wxcardlog` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_wxshop`
--

DROP TABLE IF EXISTS `mall_wxshop`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_wxshop` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ShopId` bigint(20) NOT NULL COMMENT '店铺ID',
  `AppId` varchar(30) NOT NULL COMMENT '公众号的APPID',
  `AppSecret` varchar(35) NOT NULL COMMENT '公众号的AppSecret',
  `Token` varchar(30) NOT NULL COMMENT '公众号的Token',
  `FollowUrl` varchar(500) DEFAULT NULL COMMENT '跳转的URL',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_wxshop`
--

LOCK TABLES `mall_wxshop` WRITE;
/*!40000 ALTER TABLE `mall_wxshop` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_wxshop` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mall_wxsmallchoiceproduct`
--

DROP TABLE IF EXISTS `mall_wxsmallchoiceproduct`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mall_wxsmallchoiceproduct` (
  `ProductId` int(11) NOT NULL,
  PRIMARY KEY (`ProductId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mall_wxsmallchoiceproduct`
--

LOCK TABLES `mall_wxsmallchoiceproduct` WRITE;
/*!40000 ALTER TABLE `mall_wxsmallchoiceproduct` DISABLE KEYS */;
/*!40000 ALTER TABLE `mall_wxsmallchoiceproduct` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2018-11-16 18:03:12
