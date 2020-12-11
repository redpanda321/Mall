function DeleteSlideImage2(slideImageId) {
    $.dialog.confirm('确定要删除吗？', function () {
        var loading = showLoading();
        $.post('/admin/NearShopBranch/DeleteSlideImage', { id: slideImageId }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips("删除成功");
                setTimeout(function () {
                    initSlideImagesTable2();
                }, 1500);
            }
            else
                $.dialog.errorTips('删除失败！' + result.msg);
        });
    });

}

$(function () {
    initSlideImagesTable2();

    $("#area-selector2").RegionSelector({
        selectClass: "input-sm select-sort-auto",
        valueHidden: "#AddressId2",
        maxLevel: 2
    });

    $("#slideImagesTable2").on("click", '.glyphicon-circle-arrow-up', function () {
        var oriRowNumber = parseInt($(this).attr('rowNumber'));
        var rowIndex = parseInt($(this).attr('rowIndex'));
        if (rowIndex > 0) {
            var newRowNumber = parseInt($('#slideImagesTable2 .glyphicon-circle-arrow-up[rowIndex="' + (rowIndex - 1) + '"]').attr('rowNumber'));
            changeSequence2(oriRowNumber, newRowNumber, function () {
                $("#slideImagesTable2").MallDatagrid('reload', {});
            });
        }
    }).on("click", '.glyphicon-circle-arrow-down', function () {
        var oriRowNumber = parseInt($(this).attr('rowNumber'));
        var rowIndex = parseInt($(this).attr('rowIndex'));
        var nextRow = $('#slideImagesTable2 .glyphicon-circle-arrow-up[rowIndex="' + (rowIndex + 1) + '"]');
        if (nextRow.length > 0) {
            var newRowNumber = parseInt(nextRow.attr('rowNumber'));
            changeSequence2(oriRowNumber, newRowNumber, function () {
                $("#slideImagesTable2").MallDatagrid('reload', {});
            });
        }
    });

    $("#topic2 input[type=radio]").click(function () {
        var selectValue = $(this).val();
        switch (selectValue) {
            case "1":
                $("#choceTopicUrl2 .form-group:first").hide().next().hide().next().next().hide();
                resetQuery2();
                $("#topic2 #jumpid2").html("选择门店标签").parent().next().val("").attr("readonly", "readonly");
                break;
            case "2":
                $("#choceTopicUrl2 .form-group:first").show().next().show().next().next().hide();
                resetQuery2();
                $("#topic2 #jumpid2").html("选择门店").parent().next().val("").attr("readonly", "readonly");
                break;
            case "3":
                $("#choceTopicUrl2 .form-group:first").hide().next().hide().next().next().show();
                resetQuery2();
                $("#topic2 #jumpid2").html("选择专题").parent().next().val("").attr("readonly", "readonly");
                break;
            case "4":
                $("#choceTopicUrl2 .form-group:first").hide().next().hide().next().next().hide();
                resetQuery2();
                $("#topic2 #jumpid2").html("").parent().next().val("").removeAttr("readonly");
                break;
        }
    });

    $('#topicSearchButton2').unbind('click').click(function () {
        $("#topicGrid2").MallDatagrid('reload', getQuery2());
    });
});

function getQuery2() {
    var titleKeyword = $('#titleKeyword2').val();
    var tagsKeyword = $('#tagsKeyword2').val();
    var tagsId = $('#tagsId2').val();
    var addressId = $('#AddressId2').val();
    return { tagsKeyword: tagsKeyword, titleKeyword: titleKeyword, tagsId: tagsId, addressId: addressId };
}

function resetQuery2() {
    $("#area-selector2").RegionSelector("Reset");
    $("#titleKeyword2").val("");
    $("#tagsKeyword2").val("");
    $("#tagsId2").val(0);
}

function changeSequence2(oriRowNumber, newRowNumber, callback) {
    var loading = showLoading();
    var result = false;
    $.ajax({
        type: 'post',
        url: '/admin/NearShopBranch/SlideImageChangeSequence/' + slideType2,
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

function reDrawArrow2(obj) {
    $(obj).parents('tbody').find('.glyphicon').removeClass('disabled');
    $(obj).parents('tbody').find('tr').first().find('.glyphicon-circle-arrow-up').addClass('disabled');
    $(obj).parents('tbody').find('tr').last().find('.glyphicon-circle-arrow-down').addClass('disabled');
}

function initSlideImagesTable2() {
    //商品表格
    $("#slideImagesTable2").MallDatagrid({
        url: '/admin/NearShopBranch/GetSlideImages/' + slideType2,
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
                    var html = '<img width="75" height="21" src="' + value + '" />';
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
                                    <a class="good-check" onclick="editSlideImage2(' + value + ',\'' + row.description + '\',\'' + row.url + '\',\'' + row.imgUrl + '\')">编辑</a>\
                                    <a class="good-check" onclick="DeleteSlideImage2('+ value + ')">删除</a>\
                                </span>';
                }
            }
        ]],
        onLoadSuccess: function () {
            $("#slideImagesTable2 tbody tr").first().find('.glyphicon-circle-arrow-up').addClass('disabled');
            $("#slideImagesTable2 tbody tr").last().find('.glyphicon-circle-arrow-down').addClass('disabled');
        }
    });
}


$('#addSlideImage2').click(function () {
    editSlideImage2();
});



function editSlideImage2(id, description, url, imageUrl) {
    $.dialog({
        title: (id ? '编辑' : '新增') + '轮播图',
        lock: true,
        padding: '0 40px',
        width: 480,
        id: 'editSlideImage2',
        content: $('#editSlideImage2')[0],
        okVal: '保存',
        ok: function () {
            return submitSlideImage2();
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
        $("#topic2 input[name=selectRadio][value=" + category + "]").click();
        $('#SlideImageBox2').val(imageUrl);
        $('#topic2 #selectUrl2').val(url);
        $('#description2').val(description);
        $('#SlideImageId2').val(id);
    }
    else {
        $('#SlideImageId2').val('');
        $('#SlideImageBox2').val('');
        $('#topic2 #selectUrl2').val('');
        $('#description2').val('');
        $("#topic2 input[name=selectRadio][value=1]").click();
    }

    $("#imgUrl2").MallUpload({
        title: '轮播图片：',
        imageDescript: '请上传750 * 216的图片',
        displayImgSrc: $('#SlideImageBox2').val(),
        imgFieldName: "imgUrl",
        dataWidth: 8
    });

}



function chooseTopicJumpUrl2() {
    var selectValue = $("#topic2 input[type=radio]:checked").val();
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
        id: 'chooseTopicDialog2',
        content: $('#choceTopicUrl2')[0],
        okVal: '保存',
        ok: function () {
            return saveChooseTopicToSlideImg2(jumpUrl, title);
        }
    });

    var finaly = -1;

    url = $("#topic2 #selectUrl2").val();

    if (url.length > 0) {
        var topicId = url.split("/");
        finaly = topicId[topicId.length - 1];
    }

    //商品表格
    $("#topicGrid2").MallDatagrid({
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
        queryParams: getQuery2(),
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

    $("#topicGrid2 th:last").html(desc);
}

function saveChooseTopicToSlideImg2(jumpurl, title) {
    var selectedId = $('input[name="topic"]:checked').attr('topicId');
    if (!selectedId)
        $.dialog.tips('请选择' + title);
    else {
        $("#topic2 #selectUrl2").val(jumpurl.replace("{0}", selectedId));
    }
}

function generateSlideImageInfo2() {
    //专题对象
    var slideImage = {
        id: null,
        description: null,
        imageUrl: null,
        url: null,
    };
    slideImage.id = $('#SlideImageId2').val();
    slideImage.description = $('#description2').val();
    slideImage.imageUrl = $("#imgUrl2").MallUpload('getImgSrc');
    slideImage.url = $("#topic2 #selectUrl2").val();
    slideImage.category = $("#topic2 input[type=radio]:checked").val();

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



function submitSlideImage2() {
    var returnResult = false;
    var object;
    try {
        object = generateSlideImageInfo2();
        if (!object.imageUrl)
            $.dialog.tips("请上传轮播图片");
        else {
            var objectString = JSON.stringify(object);
            var loading = showLoading();
            $.ajax({
                type: "post",
                url: '/admin/NearShopBranch/AddSlideImage',
                data: { id: object.id, description: object.description, imageUrl: object.imageUrl, url: object.category + "," + object.url, slideTypeId: slideType2 },
                dataType: "json",
                async: false,
                success: function (result) {
                    loading.close();
                    if (result.success) {
                        returnResult = true;
                        $.dialog.tips('保存成功');
                        setTimeout(function () {
                            initSlideImagesTable2();
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

