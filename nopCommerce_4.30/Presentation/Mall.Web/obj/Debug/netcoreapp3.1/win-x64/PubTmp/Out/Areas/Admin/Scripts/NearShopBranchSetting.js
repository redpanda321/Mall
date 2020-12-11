function DeleteSlideImage(slideImageId) {
    $.dialog.confirm('确定要删除吗？', function () {
        var loading = showLoading();
        $.post('/admin/NearShopBranch/DeleteSlideImage', { id: slideImageId }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips("删除成功");
                setTimeout(function () {
                    initSlideImagesTable();
                }, 1500);
            }
            else
                $.dialog.errorTips('删除失败！' + result.msg);
        });
    });

}

$(function () {
    initSlideImagesTable();
    //initShowSyncStatus();

    $("#area-selector").RegionSelector({
        selectClass: "input-sm select-sort-auto",
        valueHidden: "#AddressId",
        maxLevel: 2
    });

    $("#slideImagesTable").on("click", '.glyphicon-circle-arrow-up', function () {
        var oriRowNumber = parseInt($(this).attr('rowNumber'));
        var rowIndex = parseInt($(this).attr('rowIndex'));
        if (rowIndex > 0) {
            var newRowNumber = parseInt($('#slideImagesTable .glyphicon-circle-arrow-up[rowIndex="' + (rowIndex - 1) + '"]').attr('rowNumber'));
            changeSequence(oriRowNumber, newRowNumber, function () {
                $("#slideImagesTable").MallDatagrid('reload', {});
            });
        }
    });


    $("#slideImagesTable").on("click", '.glyphicon-circle-arrow-down', function () {
        var oriRowNumber = parseInt($(this).attr('rowNumber'));
        var rowIndex = parseInt($(this).attr('rowIndex'));
        var nextRow = $('#slideImagesTable .glyphicon-circle-arrow-up[rowIndex="' + (rowIndex + 1) + '"]');
        if (nextRow.length > 0) {
            var newRowNumber = parseInt(nextRow.attr('rowNumber'));
            changeSequence(oriRowNumber, newRowNumber, function () {
                $("#slideImagesTable").MallDatagrid('reload', {});
            });
        }
    });

    //$(".lbl-sync input[type='checkbox']").on("click", function () {
    //    var _t = $(this);
    //    var dtype = _t.data("type");
    //    var v = _t.is(":checked");
    //    var loading = showLoading();
    //    $.ajax({
    //        type: 'post',
    //        url: '/admin/NearShopBranch/ShopBranchSettingUpdateAppletSyncStatus/',
    //        data: { type: dtype, status: v },
    //        dataType: "json",
    //        success: function (data) {
    //            loading.close();
    //            if (data.success) {
    //                switch (dtype) {
    //                    case 1:
    //                        appletSyncData.topSlide = v;
    //                        break;
    //                    case 2:
    //                        appletSyncData.iconArea = v;
    //                        break;
    //                    case 3:
    //                        appletSyncData.adArea = v;
    //                        break;
    //                    case 4:
    //                        appletSyncData.middleSlide = v;
    //                        break;
    //                }
    //                //initShowSyncStatus();
    //            } else {
    //                $.dialog.tips('操作失败：' + data.msg);
    //            }
    //        },
    //        error: function () {
    //            loading.close();
    //        }
    //    });
    //});
    //initShowSyncStatus();

    $("#topic input[type=radio]").click(function () {
        var selectValue = $(this).val();
        switch (selectValue) {
            case "1":
                $("#choceTopicUrl .form-group:first").hide().next().hide().next().next().hide();
                resetQuery();
                $("#topic #jumpid").html("选择门店标签").parent().next().val("").attr("readonly", "readonly");
                break;
            case "2":
                $("#choceTopicUrl .form-group:first").show().next().show().next().next().hide();
                resetQuery();
                $("#topic #jumpid").html("选择门店").parent().next().val("").attr("readonly", "readonly");
                break;
            case "3":
                $("#choceTopicUrl .form-group:first").hide().next().hide().next().next().show();
                resetQuery();
                $("#topic #jumpid").html("选择专题").parent().next().val("").attr("readonly", "readonly");
                break;
            case "4":
                $("#choceTopicUrl .form-group:first").hide().next().hide().next().next().hide();
                resetQuery();
                $("#topic #jumpid").html("").parent().next().val("").removeAttr("readonly");
                break;
        }
    });

    $('#topicSearchButton').unbind('click').click(function () {
        $("#topicGrid").MallDatagrid('reload', getQuery());
    });
});

//function initShowSyncStatus() {
//    $("#o2o_sync_top_slide").prop("checked", appletSyncData.topSlide);
//    $("#o2o_sync_icon_area").prop("checked", appletSyncData.iconArea);
//    $("#o2o_sync_ad_area").prop("checked", appletSyncData.adArea);
//    $("#o2o_sync_middle_slide").prop("checked", appletSyncData.middleSlide);
//}

function getQuery() {
    var titleKeyword = $('#titleKeyword').val();
    var tagsKeyword = $('#tagsKeyword').val();
    var tagsId = $('#tagsId').val();
    var addressId = $('#AddressId').val();
    return { tagsKeyword: tagsKeyword, titleKeyword: titleKeyword, tagsId: tagsId, addressId: addressId };
}

function resetQuery() {
    $("#area-selector").RegionSelector("Reset");
    $("#titleKeyword").val("");
    $("#tagsKeyword").val("");
    $("#tagsId").val(0);
}

function changeSequence(oriRowNumber, newRowNumber, callback) {
    var loading = showLoading();
    var result = false;
    $.ajax({
        type: 'post',
        url: '/admin/NearShopBranch/SlideImageChangeSequence/' + slideType,
        cache: false,
        async: true,
        data: { oriRowNumber: oriRowNumber, newRowNumber: newRowNumber },
        dataType: "json",
        success: function (data) {
            loading.close();
            if (!data.success)
                $.dialog.tips('调整顺序出错!' + data.msg);
            else
                callback();
        },
        error: function () {
            loading.close();
        }
    });
}

function reDrawArrow(obj) {
    $(obj).parents('tbody').find('.glyphicon').removeClass('disabled');
    $(obj).parents('tbody').find('tr').first().find('.glyphicon-circle-arrow-up').addClass('disabled');
    $(obj).parents('tbody').find('tr').last().find('.glyphicon-circle-arrow-down').addClass('disabled');
}

function initSlideImagesTable() {
    //商品表格
    $("#slideImagesTable").MallDatagrid({
        url: '/admin/NearShopBranch/GetSlideImages/' + slideType,
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到任何轮播图',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: false,
        idField: "id",
        pagePosition: 'bottom',
        columns:
        [[
            {
                field: "imgUrl", title: '缩略图', align: "center", width: 80, formatter: function (value, row, index) {
                    var html = '<img width="75" height="42" src="' + value + '" />';
                    return html;
                }
            },
            {
                field: "description", title: '描述', align: "center", width: 120, formatter: function (value, row, index) {
                    return '<span class="overflow-ellipsis" style="width:120px">' + value + '</span> ';
                },
            },
            {
                field: "url", title: '跳转链接', align: "center", width: 180, formatter: function (value, row, index) {
                    var text = "";
                    var category = 0;
                    if (value.length > 0) {
                        var arr = value.split(",");
                        if (arr.length = 3) {
                            category = arr[0];
                            value = arr[1];
                            text = arr[2];
                        }
                    }
                    switch (category) {
                        case "1":
                            text = "门店标签:" + text;
                            break;
                        case "2":
                            text = "门店:" + text;
                            break;
                        case "3":
                            text = "专题:" + text;
                            break;
                        case "4":
                            text = value;
                            break;
                    }
                    return '<a href="' + value + '" target="_blank"><span class="overflow-ellipsis" style="width:180px">' + text + '</span></a>';
                },
            },
            {
                field: "displaySequence", title: '排序', align: "center", formatter: function (value, row, index) {
                    return '<span class="glyphicon glyphicon-circle-arrow-up" rownumber="' + value + '" rowIndex="' + index + '"></span> <span class="glyphicon glyphicon-circle-arrow-down" rownumber="' + value + '" rowIndex="' + index + '"></span>';
                }
            },
            {
                field: "id", title: '操作', align: "center", formatter: function (value, row, index) {
                    return ' <span class="btn-a">\
                                    <a class="good-check" onclick="editSlideImage(' + value + ',\'' + row.description + '\',\'' + row.url + '\',\'' + row.imgUrl + '\')">编辑</a>\
                                    <a class="good-check" onclick="DeleteSlideImage('+ value + ')">删除</a>\
                                </span>';
                }
            }
        ]],
        onLoadSuccess: function () {
            $("#slideImagesTable tbody tr").first().find('.glyphicon-circle-arrow-up').addClass('disabled');
            $("#slideImagesTable tbody tr").last().find('.glyphicon-circle-arrow-down').addClass('disabled');
        }
    });
}


$('#addSlideImage').click(function () {
    editSlideImage();
});



function editSlideImage(id, description, url, imageUrl) {
    $.dialog({
        title: (id ? '编辑' : '新增') + '轮播图',
        lock: true,
        padding: '0 40px',
        width: 480,
        id: 'editSlideImage',
        content: $('#editSlideImage')[0],
        okVal: '保存',
        ok: function () {
            return submitSlideImage();
        }
    });
    var category = 1;
    if (url && url.length > 0) {
        var arr = url.split(",");
        if (arr.length = 2) {
            category = arr[0];
            url = arr[1];
        }
    }
    if (id) {
        $("#topic input[name=selectRadio][value=" + category + "]").click();
        $('#SlideImageBox').val(imageUrl);
        $('#topic #selectUrl').val(url);
        $('#description').val(description);
        $('#SlideImageId').val(id);
    }
    else {
        $('#SlideImageId').val('');
        $('#SlideImageBox').val('');
        $('#topic #selectUrl').val('');
        $('#description').val('');
        $("#topic input[name=selectRadio][value=1]").click();
    }

    $("#imgUrl").MallUpload({
        title: '轮播图片：',
        imageDescript: '请上传750 * 426的图片',
        displayImgSrc: $('#SlideImageBox').val(),
        imgFieldName: "imgUrl",
        dataWidth: 8
    });

}



function chooseTopicJumpUrl() {
    var selectValue = $("#topic input[type=radio]:checked").val();
    var title = "门店标签";
    var desc = "描述";
    var queryUrl = "/admin/NearShopBranch/ShopTagList";
    var jumpUrl = "#";
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

    $.dialog({
        title: '选择' + title,
        lock: true,
        width: 550,
        padding: '0 5px',
        id: 'chooseTopicDialog',
        content: $('#choceTopicUrl')[0],
        okVal: '保存',
        ok: function () {
            return saveChooseTopicToSlideImg(jumpUrl, title);
        }
    });

    var finaly = -1;

    url = $("#topic #selectUrl").val();

    if (url.length > 0) {
        var topicId = url.split("/");
        finaly = topicId[topicId.length - 1];
    }

    //商品表格
    $("#topicGrid").MallDatagrid({
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
        queryParams: getQuery(),
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

    $("#topicGrid th:last").html(desc);
}

function saveChooseTopicToSlideImg(jumpurl, title) {
    var selectedId = $('input[name="topic"]:checked').attr('topicId');
    if (!selectedId)
        $.dialog.tips('请选择' + title);
    else {
        $("#topic #selectUrl").val(jumpurl.replace("{0}", selectedId));
    }
}

function generateSlideImageInfo() {
    //专题对象
    var slideImage = {
        id: null,
        description: null,
        imageUrl: null,
        url: null,
    };
    slideImage.id = $('#SlideImageId').val();
    slideImage.description = $('#description').val();
    slideImage.imageUrl = $("#imgUrl").MallUpload('getImgSrc');
    slideImage.url = $("#topic #selectUrl").val();
    slideImage.category = $("#topic input[type=radio]:checked").val();

    if (!slideImage.imageUrl)
        throw new Error('请上传图片');
    if (slideImage.description.length > 10)
        throw new Error('描述信息在10个字符以内');
    if (!slideImage.url)
        throw new Error('请输入链接地址');
    if (slideImage.url.toLowerCase().indexOf('http://') < 0 && slideImage.url.toLowerCase().indexOf('https://') < 0 && slideImage.url.charAt(0) != '/')
        throw new Error('链接地址请以"http://"或"https://"开头');

    !slideImage.id && (slideImage.id = 0);
    return slideImage;
}



function submitSlideImage() {
    var returnResult = false;
    var object;
    try {
        object = generateSlideImageInfo();
        if (!object.imageUrl)
            $.dialog.tips("请上传轮播图片");
        else {
            var objectString = JSON.stringify(object);
            var loading = showLoading();
            $.ajax({
                type: "post",
                url: '/admin/NearShopBranch/AddSlideImage',
                data: { id: object.id, description: object.description, imageUrl: object.imageUrl, url: object.category + "," + object.url, slideTypeId: slideType },
                dataType: "json",
                async: false,
                success: function (result) {
                    loading.close();
                    if (result.success) {
                        returnResult = true;
                        $.dialog.tips('保存成功');
                        setTimeout(function () {
                            initSlideImagesTable();
                        }, 1500);
                    }
                    else
                        $.dialog.tips('保存失败！' + result.msg);
                }
            });
        }
    }
    catch (e) {
        $.dialog.errorTips('保存失败!' + e.message);
    }
    return returnResult;
}

