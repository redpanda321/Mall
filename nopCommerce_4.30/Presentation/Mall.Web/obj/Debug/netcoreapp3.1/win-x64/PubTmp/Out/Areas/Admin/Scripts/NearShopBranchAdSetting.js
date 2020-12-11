$(function () {
    $("#area-selectorAd").RegionSelector({
        selectClass: "input-sm select-sort-auto",
        valueHidden: "#AddressIdAd",
        maxLevel: 2
    });

    $("#imgad input[type=radio]").click(function () {
        var selectValue = $(this).val();
        switch (selectValue) {
            case "1":
                $("#chooseAdJumpUrl .form-group:first").hide().next().hide().next().next().hide();
                resetAdQuery();
                $("#imgad #jumpid").html("选择门店标签").parent().next().val("").attr("readonly", "readonly");
                break;
            case "2":
                $("#chooseAdJumpUrl .form-group:first").show().next().show().next().next().hide();
                resetAdQuery();
                $("#imgad #jumpid").html("选择门店").parent().next().val("").attr("readonly", "readonly");
                break;
            case "3":
                $("#chooseAdJumpUrl .form-group:first").hide().next().hide().next().next().show();
                resetAdQuery();
                $("#imgad #jumpid").html("选择专题").parent().next().val("").attr("readonly", "readonly");
                break;
            case "4":
                $("#chooseAdJumpUrl .form-group:first").hide().next().hide().next().next().hide();
                resetAdQuery();
                $("#imgad #jumpid").html("").parent().next().val("").removeAttr("readonly");
                break;
        }
    });

    $('#ImgAdQuery').unbind('click').click(function () {
        $("#ImgAdGrid").MallDatagrid('reload', getAdQuery());
    });
});

function getAdQuery() {
    var titleKeyword = $('#queryTitleAd').val();
    var tagsKeyword = $('#queryTagsAd').val();
    var tagsId = $('#tagsIdAd').val();
    var addressId = $('#AddressIdAd').val();
    return { tagsKeyword: tagsKeyword, titleKeyword: titleKeyword, tagsId: tagsId, addressId: addressId };
}

function resetAdQuery() {
    $("#area-selectorAd").RegionSelector("Reset");
    $("#queryTitleAd").val("");
    $("#queryTagsAd").val("");
    $("#tagsIdAd").val(0);
}

function chooseImgAdJumpUrl() {
    var selectValue = $("#imgad input[type=radio]:checked").val();
    var title = "门店标签";
    var queryUrl = "/admin/NearShopBranch/ShopTagList";
    var jumpUrl = "#";
    var desc = "描述";
    var prohp = document.location.protocol;//表示当前的网络协议

    switch (selectValue) {
        case "1":
            queryUrl = "/admin/NearShopBranch/ShopTagList";
            title = "门店标签";
            desc = "描述";
            jumpUrl = prohp + "//" + window.location.host + "/m-wap/ShopBranch/Tags/{0}";
            break;
        case "2":
            queryUrl = "/admin/NearShopBranch/ShopList";
            title = "门店";
            desc = "标签";
            jumpUrl = prohp + "//" + window.location.host + "/m-wap/ShopBranch/Index/{0}";
            break;
        case "3":
            queryUrl = "/admin/MobileTopic/list";
            title = "专题";
            desc = "标签";
            jumpUrl = prohp + "//" + window.location.host + "/m-wap/topic/detail/{0}";
            break;
    }

    var finaly = -1;

    url = $("#imgad #selectUrl").val();

    if (url.length > 0) {
        var topicId = url.split("/");
        finaly = topicId[topicId.length - 1];
    }

    //商品表格
    $("#ImgAdGrid").MallDatagrid({
        url: queryUrl,
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pageSize: 10,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { auditStatus: 1 },
        columns:
        [[
            {
                field: "id", title: '选择', align: "center", width: 80, formatter: function (value, row, index) {
                    if (row.id == parseInt(finaly)) {
                        return '<input type="radio" name="topic" topicId="' + value + '" checked="checked"/>';
                    }
                    var html = '<input type="radio" name="topic" topicId="' + value + '" />';
                    return html;
                }
            },
            {
                field: "name", title: '名称', align: "center", formatter: function (value, row, index) {
                    var html = '<a href="' + jumpUrl.replace("{0}", row.id) + '" target="_blank">' + row.name + '</a>';
                    return html;
                }
            },
            {
                field: "tags", title: desc, align: "center"
            }
        ]]
    });

    $("#ImgAdGrid th:last").html(desc);

    $.dialog({
        title: '选择跳转' + title,
        lock: true,
        width: 550,
        padding: '0 5px',
        id: 'chooseImaAdDialog',
        content: $('#chooseAdJumpUrl')[0],
        okVal: '保存',
        ok: function () {
            return saveChooseImaAd(jumpUrl, title);
        }
    });
}

function saveChooseImaAd(jumpurl, title) {
    var selectedId = $('input[name="topic"]:checked').attr('topicId');
    if (!selectedId)
        $.dialog.tips('请选择' + title);
    else {
        $("#imgad #selectUrl").val(jumpurl.replace("{0}", selectedId));
    }
}

function editImageAd(target, id, category) {
    $('#AdImageBox').val('');
    $('#imgad #selectUrl').val('');
    $("#imgad input[name=selectRadio][value=1]").click();

    getSlideImage(id);
    $.dialog({
        title: ('编辑广告位图'),
        lock: true,
        padding: '0 40px',
        width: 450,
        id: 'editImageAds',
        content: $('#editImageAds')[0],
        okVal: '保存',
        ok: function () {
            return submitImageAd(target, id);
        }
    });
    var Descript = category == 1 ? "请上传318 * 480的图片" : "请上传215 * 240的图片";

    $("#uploadImgUrl").MallUpload({
        title: '专题图片：',
        imageDescript: Descript,
        displayImgSrc: $('#AdImageBox').val(),
        imgFieldName: "uploadImgUrl",
        dataWidth: 8
    });
}
function submitImageAd(target, id) {
    var imageUrl = $('#uploadImgUrl').MallUpload('getImgSrc');
    var url = $("#imgad #selectUrl").val();
    var category = $("#imgad input[type=radio]:checked").val();
    $.ajax({
        type: "post",
        url: '/admin/NearShopBranch/UpdateImageAd',
        data: { id: id, pic: imageUrl, url: category + "," + url },
        dataType: "json",
        async: false,
        success: function (result) {
            if (result.success) {
                $(target).children().first().attr("src", result.data.ImageUrl);
                $.dialog.tips('保存成功');
            }
            else
                $.dialog.tips('保存失败！' + result.msg);
        }
    });
}

function getSlideImage(id) {
    $.ajax({
        type: "GET",
        url: '/admin/NearShopBranch/GetImageAd/' + id,
        dataType: "json",
        async: false,
        success: function (result) {
            if (result.success) {
                var category = 1;
                var url = result.url;
                if (url && url.length > 0) {
                    var arr = url.split(",");
                    if (arr.length = 2) {
                        category = arr[0];
                        url = arr[1];
                    }
                }
                $("#imgad input[name=selectRadio][value=" + category + "]").click();
                $("#imgad #selectUrl").val(url);
                $('#AdImageBox').val(result.imageUrl);
            }
            else
                $.dialog.tips("获取图片失败" + result.msg);
        }
    });
}

