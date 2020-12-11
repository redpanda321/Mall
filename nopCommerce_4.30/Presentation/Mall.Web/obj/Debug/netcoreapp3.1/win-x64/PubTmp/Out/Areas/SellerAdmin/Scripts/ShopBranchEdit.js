$(function () {
    $("#area-selector").RegionSelector({
        selectClass: "input-sm select-sort-auto",
        valueHidden: "#AddressId"
    });
    
    InitStoreBanner();//初始化门店Banner
    initMap();//初始化门店地图
   
    $("#cbxAboveSelf").click(function () {
        $("#IsAboveSelf").val($(this).prop("checked"));
    });
    $("#cbxStoreDelive").click(function () {
        var target = $(this);
        var flag = $(this).prop("checked");
        $("#IsStoreDelive").val(flag);
        if (flag) {
            target.parents("tr").next().show().next().show().next().show().next().show().next().show();
        } else {
            $("#ServeRadius").val(0);
            $("#DeliveFee").val(0);
            $("#DeliveTotalFee").val(0);
            $("#FreeMailFee").val(0);
            target.parents("tr").next().hide().next().hide().next().hide().next().hide().next().hide();
        }
    });
    if ($("#IsAboveSelf").val().toLowerCase() == "true") {
        $("#cbxAboveSelf").prop("checked", "checked");
    }
    if ($("#IsStoreDelive").val().toLowerCase() == "true") {
        $("#cbxStoreDelive").prop("checked", "checked").parents("tr").next().show().next().show().next().show().next().show();
    }
    if ($("#StoreOpenStartTime").val()) {
        var d = new Date("2017/8/16 " +$("#StoreOpenStartTime").val());
        $("#startTimeH").val(d.getHours());
        $("#startTimeM").val(d.getMinutes());
    }
    if ($("#StoreOpenEndTime").val()) {
        var d = new Date("2017/8/16 " +$("#StoreOpenEndTime").val());
        $("#endTimeH").val(d.getHours());
        $("#endTimeM").val(d.getMinutes());
    }
    $("#DeliveFee,#FreeMailFee,#DeliveTotalFee").keyup(function () {
        var val = $(this).val().replace(/[^0-9]+/, '').replace(/\b(0+)/gi, '');
        $(this).val(val ? val : 0);
    }).blur(function () {
        var val = $(this).val().replace(/[^0-9]+/, '').replace(/\b(0+)/gi, '');
        $(this).val(val ? val : 0);
    });
    $("#ServeRadius").keyup(function () {
        var val = $(this).val().replace(/[^0-9]+/, '').replace(/\b(0+)/gi, '');
        $(this).val(val ? val : 1);
    }).blur(function () {
        var val = $(this).val().replace(/[^0-9]+/, '').replace(/\b(0+)/gi, '');
        $(this).val(val ? val : 1);
    });
    $("input[name='IsFreeMailFee']").click(function () {
        if (this.value == 1)
            $("#IsFreeMail").val(true);
        else
            $("#IsFreeMail").val(false);
    });
    $("#FreeMailFee").focus(function () {
        $("#rdoFreeMail2").attr("checked", "checked");
        $("#IsFreeMail").val(true);
    });

    var flag = $("#cbxStoreDelive").prop("checked");
    $("#IsStoreDelive").val(flag);
    if (flag) {
        $("#cbxStoreDelive").parents("tr").next().show().next().show().next().show().next().show().next().show();
    } else {
        $("#ServeRadius").val(0);
        $("#DeliveFee").val(0);
        $("#DeliveTotalFee").val(0);
        $("#FreeMailFee").val(0);
        $("#cbxStoreDelive").parents("tr").next().hide().next().hide().next().hide().next().hide().next().hide();
    }
});
function checkUsernameIsValid(username) {
    var result = false;
    var normalreg = /^([\u4E00-\u9FA5]|[A-Za-z0-9])[\u4E00-\u9FA5\A-Za-z0-9\_\-]{3,19}$/;
    var reg = normalreg;
    if (!username || username == '用户名') {
        $.dialog.tips('管理员账号输入有误');
        return result;
    }
    if ((/^\d+$/.test(username))) {
        $.dialog.tips('不可以使用纯数字管理员账号');
        return result;
    }
    if (!reg.test(username)) {
        $.dialog.tips('管理员账号需要4-20位字符，支持中英文、数字及"-"、"_"的组合');
        return result;
    }
    result = true;
    return result;
}
function checkPasswordIsValid(password) {
    var result = false;
    var reg = /^[^\s]{6,20}$/;
    var result = reg.test(password);
    return result;
}
function checkData() {
    if ($.trim($('#ShopBranchName').val()).length == 0) {
        $.dialog.tips('门店名称不能为空');
        return;
    }
    if ($('#ShopBranchName').val().length > 15) {
        $.dialog.tips('门店名称不能超过15个字');
        return;
    }
    if ($('#AddressId').attr('isfinal') == 'false') {
        $.dialog.tips('请选择门店地址');
        return;
    }
    if ($('#AddressDetail').val().length > 50) {
        $.dialog.tips('详细地址不能超过50个字');
        return;
    }
    if (Number($("#Longitude").val()) <= 0 || Number($("#Latitude").val()) <= 0) {
        $.dialog.tips('请搜索地址地图定位');
        return;
    }
    if (!($("#cbxAboveSelf").prop("checked") || $("#cbxStoreDelive").prop("checked"))) {
        $.dialog.tips('请选择配送方式');
        return;
    }
    
    if ($("#cbxStoreDelive").prop("checked")) {
        if (Number($("#ServeRadius").val()) <= 0) {
            $.dialog.tips('选择门店配送时配送半径必须大于0');
            return;
        }
        if (Number($("#DeliveFee").val()) < 0 || Number($("#DeliveTotalFee").val()) < 0) {
            $.dialog.tips('选择门店配送时配送费和起送价不能为空');
            return;
        }
        var IsFreeMail = $('#IsFreeMail').val();
        var FreeMailFee = parseFloat($('#FreeMailFee').val());
        if (IsFreeMail == 'true' && FreeMailFee <= 0) {
            $.dialog.tips('满额包邮金额必须大于0');
            $('#FreeMailFee').focus();
            return;
        }
    }

    var phone = $("#ContactPhone").val();
    if (phone == "") {
        $.dialog.tips('请输入联系电话');
        $('#ContactPhone').focus();
        return;
    }
    if (phone.length > 15) {
        $.dialog.tips('联系电话不能超过15个字符');
        $('#ContactPhone').focus();
        return;
    }
    var isMobile = /^(((13[0-9]{1})|(15[0-9]{1})|(18[0-9]{1})|(17[0-9]{1})|(14[0-9]{1}))+\d{8})$/;
    var isPhone = /^(?:(?:0\d{2,3})-)?(?:\d{7,8})(-(?:\d{3,}))?$/;
    if (!isMobile.test(phone) && !isPhone.test(phone)) {
        $.dialog.tips('请输入正确的联系电话');
        $('#ContactPhone').focus();
        return;
    }


    if ($('input[name=PasswordOne]').val() != $('input[name=PasswordTwo]').val()) {
        $.dialog.tips('两次密码不一致');
        return;
    }
    str = $('input[name=PasswordOne]').val();
    if (str != '' && !checkPasswordIsValid(str)) {
        $.dialog.tips('密码最少6位');
        return;
    }
    $("#ShopImages").val($("#storeBanner").MallUpload('getImgSrc').toString());//门店Banner
    var startTimeH = $("#startTimeH").val();
    var startTimeM = $("#startTimeM").val();
    var endTimeH = $("#endTimeH").val();
    var endTimeM = $("#endTimeM").val();
    if (!checkTime(startTimeH, 0, 24)) {
        $.dialog.tips('营业起始时间小时必须0~23小时');
        return;
    }
    if (!checkTime(startTimeM, 0, 60)) {
        $.dialog.tips('营业起始时间分钟必须0~59分');
        return;
    }
    if (!checkTime(endTimeH, 0, 24)) {
        $.dialog.tips('营业结束时间小时必须0~23小时');
        return;
    }
    if (!checkTime(endTimeM, 0, 60)) {
        $.dialog.tips('营业结束时间分钟必须0~59分');
        return;
    }
    $("#StoreOpenStartTime").val(startTimeH + ":" + startTimeM);
    $("#StoreOpenEndTime").val(endTimeH + ":" + endTimeM);
    //门店标签
    var selectTagids = new Array();
    $('input:checkbox[name=chkTags]:checked').each(function (i) {
        selectTagids.push($(this).val());
    });
    if (selectTagids.length > 5){
        $.dialog.tips('最多勾选5个标签');
        return;
    }
    $("#ShopBranchTagId").val(selectTagids);

    var data = $('#from_Save1').serialize();
    
    //var loading = showLoading();
    $.post('Edit', data, function (data) {
        if (data.success == true) {
            var title = "修改门店成功!";
            if (data.msg && data.msg.length > 0) {
                title += data.msg;
            }
            $.dialog.tips(title, function () {
                location.href = $('#urlManagement').val();
            });
        } else {
            $.dialog.tips(data.msg);
        }
    });
}

function checkTime(source, min, max) {
    if (!isNaN(source) && source != '' && source >= min && source < max) {
        return true;
    }
    return false;
}

function InitStoreBanner(storeBanner) {
    $('#storeBanner').MallUpload({
        title: '<b>*</b>门店Banner：',
        imageDescript: '建议尺寸200*200，支持.jpg .jpeg .bmp .png格式，大小不超过1M',
        displayImgSrc: $("#displayShopImagesSrc").val(),
        imgFieldName: "storeBannerLogo",
        dataWidth: 8,
        imagesCount: 1
    });
}
var map, searchService, marker, markers = [], infoWin = null;
var initMap = function () {
    var longitude = $("#Longitude").val();
    var latitude = $("#Latitude").val();
    var center = new qq.maps.LatLng(latitude, longitude);
    map = new qq.maps.Map(document.getElementById('container'), {
        center: center,
        zoom: 13
    });
    var scaleControl = new qq.maps.ScaleControl({
        align: qq.maps.ALIGN.BOTTOM_LEFT,
        margin: qq.maps.Size(85, 15),
        map: map
    });
    marker = new qq.maps.Marker({
        position: center,
        map: map,
        draggable: true
    });
    qq.maps.event.addListener(marker, 'dragend', function () {
        if (marker.getPosition()) {
            $("#Longitude").val(marker.getPosition().getLng());
            $("#Latitude").val(marker.getPosition().getLat());
        }
    });
    //调用Poi检索类
    searchService = new qq.maps.SearchService({
        //检索成功的回调函数
        complete: function (results) {
            //设置回调函数参数
            var pois = results.detail.pois;
            infoWin = new qq.maps.InfoWindow({
                map: map
            });
            var latlngBounds = new qq.maps.LatLngBounds();
            for (var i = 0, l = pois.length; i < l; i++) {
                var poi = pois[i];
                //扩展边界范围，用来包含搜索到的Poi点
                latlngBounds.extend(poi.latLng);
                (function (n) {
                    var marker = new qq.maps.Marker({
                        map: map
                    });
                    marker.setPosition(pois[n].latLng);
                    markers.push(marker);
                    qq.maps.event.addListener(marker, 'click', function () {
                        infoWin.open();
                        infoWin.setContent('<div style = "width:200px;padding:10px 0;">' + pois[n].address + '<div class="map-import-btn"><input type="button" class="btn btn-xs btn-primary" value="导入门店地址" onclick="chooseShopLoc(this);" address=' + pois[n].address + ' lat =' + pois[n].latLng.getLat() + '  lng =' + pois[n].latLng.getLng() + ' /></div></div>');
                        infoWin.setPosition(pois[n].latLng);
                    });
                })(i);
            }
            //调整地图视野
            map.fitBounds(latlngBounds);
        },
        //若服务请求失败，则运行以下函数
        error: function () {
            alert("很抱歉，未搜索到此地址，请重新输入！");
        }
    });
}
//导入门店信息
function chooseShopLoc(t) {
    var address = $(t).attr("address");
    var storeAreaArr = getSelectArea();
    //for (var i = 0; i < storeAreaArr.length; i++) {
    //    if (i <= 3) {
    //        address = address.replace(storeAreaArr[i], '');
    //    }
    //}
    for (var i = 3; i >= 0; i--) {
        if (i == 0) {
            address = address.replace(storeAreaArr[0] + "市", '');
        }
        address = address.replace(storeAreaArr[i], '');
    }
    var lat = $(t).attr("lat");
    var lng = $(t).attr("lng");
    this.clearMarkers();
    var position = new qq.maps.LatLng(lat, lng);
    marker = new qq.maps.Marker({
        map: map,
        position: position,
        draggable: true
    });
    map.panTo(position);
    map.zoomTo(18);
    $("#Longitude").val(lng);
    $("#Latitude").val(lat);
    qq.maps.event.addListener(marker, 'dragend', function () {
        if (marker.getPosition()) {
            $("#Longitude").val(marker.getPosition().getLng());
            $("#Latitude").val(marker.getPosition().getLat());
        }
    });
    $("#AddressDetail").val(address);
    if (infoWin) {
        infoWin.close();
    }
    $("#map_des").hide();
}
////删除所有标记
function clearMarkers() {
    if (markers) {
        for (i = 0; i < markers.length; i++) {
            markers[i].setMap(null);
        }
        markers.length = 0;
    }
}
function getSelectArea() {
    var storeArr = [];
    $("#area-selector select").each(function (i) {
        if ($(this).find("option:selected").text() != '请选择') {
            storeArr.push($(this).find("option:selected").text());
        }
    });
    return storeArr;
}
//搜索地址，这里需要判断是否选择了省市区
function getResult() {
    if ($("#AddressId").val() <= 0) {
        $.dialog.tips("请先选择地区");
        return;
    }
    if ($.trim($("#AddressDetail").val()).length == 0) {
        $.dialog.tips("请先输入详细地址");
        return;
    }
    if (marker != null) marker.setMap(null);
    clearMarkers();
    if (infoWin) {
        infoWin.close();
    }
    var storeArr = getSelectArea();
    var regions = storeArr[0] + storeArr[1] + storeArr[2];
    var regionText = storeArr.join(',');
    var poiText = regions + $.trim($("#AddressDetail").val());
    searchService.setLocation(regionText);
    searchService.search(poiText);
    $("#map_des").show();
}
