// JavaScript source code
function DeleteNSBIcon(slideImageId) {
    $.dialog.confirm('确定要删除吗？', function () {
        var loading = showLoading();
        $.post('/admin/NearShopBranch/DeleteNearShopBranchIcon', { id: slideImageId }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips("删除成功");
                setTimeout(function () {
                    initNSBIconTable();
                }, 1500);
            }
            else
                $.dialog.errorTips('删除失败！' + result.msg);
        });
    });

}

$(function () {
    initNSBIconTable();

    $("#area-selectorIcon").RegionSelector({
        selectClass: "input-sm select-sort-auto",
        valueHidden: "#AddressIdIcon",
        maxLevel: 2
    });

    $("#NSBIconTable").on("click", '.glyphicon-circle-arrow-up', function () {
        var oriRowNumber = parseInt($(this).attr('rowNumber'));
        var rowIndex = parseInt($(this).attr('rowIndex'));
        if (rowIndex > 0) {
            var newRowNumber = parseInt($('#NSBIconTable .glyphicon-circle-arrow-up[rowIndex="' + (rowIndex - 1) + '"]').attr('rowNumber'));
            changeNSBIconSequence(oriRowNumber, newRowNumber, function () {
                $("#NSBIconTable").MallDatagrid('reload', {});
            });
        }
    });


    $("#NSBIconTable").on("click", '.glyphicon-circle-arrow-down', function () {
        var oriRowNumber = parseInt($(this).attr('rowNumber'));
        var rowIndex = parseInt($(this).attr('rowIndex'));
        var nextRow = $('#NSBIconTable .glyphicon-circle-arrow-up[rowIndex="' + (rowIndex + 1) + '"]');
        if (nextRow.length > 0) {
            var newRowNumber = parseInt(nextRow.attr('rowNumber'));
            changeNSBIconSequence(oriRowNumber, newRowNumber, function () {
                $("#NSBIconTable").MallDatagrid('reload', {});
            });
        }
    });

    $("#icon input[type=radio]").click(function () {
        var selectValue = $(this).val();
        switch (selectValue) {
            case "1":
                $("#chooseJumpUrl .form-group:first").hide().next().hide().next().next().hide();
                resetIconQuery();
                $("#icon #jumpid").html("选择门店标签").parent().next().val("").attr("readonly", "readonly");
                break;
            case "2":
                $("#chooseJumpUrl .form-group:first").show().next().show().next().next().hide();
                resetIconQuery();
                $("#icon #jumpid").html("选择门店").parent().next().val("").attr("readonly", "readonly");
                break;
            case "3":
                $("#chooseJumpUrl .form-group:first").hide().next().hide().next().next().show();
                resetIconQuery();
                $("#icon #jumpid").html("选择专题").parent().next().val("").attr("readonly", "readonly");
                break;
            case "4":
                $("#chooseJumpUrl .form-group:first").hide().next().hide().next().next().hide();
                resetIconQuery();
                $("#icon #jumpid").html("").parent().next().val("").removeAttr("readonly");
                break;
        }
    });

    $('#queryButton').unbind('click').click(function () {
        $("#jumpUrlGrid").MallDatagrid('reload', getIconQuery());
    });
});

function getIconQuery() {
    var titleKeyword = $('#queryTitle').val();
    var tagsKeyword = $('#queryTags').val();
    var tagsId = $('#tagsIdIcon').val();
    var addressId = $('#AddressIdIcon').val();
    return { tagsKeyword: tagsKeyword, titleKeyword: titleKeyword, tagsId: tagsId, addressId: addressId };
}

function resetIconQuery() {
    $("#area-selectorIcon").RegionSelector("Reset");
    $("#queryTitle").val("");
    $("#queryTags").val("");
    $("#tagsIdIcon").val(0);
}

function changeNSBIconSequence(oriRowNumber, newRowNumber, callback) {
    var loading = showLoading();
    var result = false;
    $.ajax({
        type: 'post',
        url: '/admin/NearShopBranch/NSBIconChangeSequence',
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

//function reDrawArrow(obj) {
//    $(obj).parents('tbody').find('.glyphicon').removeClass('disabled');
//    $(obj).parents('tbody').find('tr').first().find('.glyphicon-circle-arrow-up').addClass('disabled');
//    $(obj).parents('tbody').find('tr').last().find('.glyphicon-circle-arrow-down').addClass('disabled');
//}

function initNSBIconTable() {
    //商品表格
    $("#NSBIconTable").MallDatagrid({
        url: '/admin/NearShopBranch/GetNearShopBranchIcons',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到任何图标',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: false,
        idField: "id",
        pagePosition: 'bottom',
        columns:
        [[
            {
                field: "imgUrl", title: '缩略图', align: "center", width: 100, formatter: function (value, row, index) {
                    var html = '<img width="86" height="86" src="' + value + '" />';
                    return html;
                }
            },
            {
                field: "description", title: '图标名称', align: "center", width: 120, formatter: function (value, row, index) {
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
                                    <a class="good-check" onclick="editNSBIcon(' + value + ',\'' + row.description + '\',\'' + row.url + '\',\'' + row.imgUrl + '\')">编辑</a>\
                                    <a class="good-check" onclick="DeleteNSBIcon('+ value + ')">删除</a>\
                                </span>';
                }
            }
        ]],
        onLoadSuccess: function () {
            $("#NSBIconTable tbody tr").first().find('.glyphicon-circle-arrow-up').addClass('disabled');
            $("#NSBIconTable tbody tr").last().find('.glyphicon-circle-arrow-down').addClass('disabled');
        }
    });
}


$('#addNSBIcon').click(function () {
    editNSBIcon();
});



function editNSBIcon(id, description, url, imageUrl) {
    $.dialog({
        title: (id ? '编辑' : '新增') + '图标',
        lock: true,
        padding: '0 20px',
        width: 480,
        id: 'editNSBIcon',
        content: $('#editNSBIcon')[0],
        okVal: '保存',
        ok: function () {
            return submitNSBIcon();
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
        $("#icon input[name=selectRadio][value=" + category + "]").click();
        $('#NSBIconBox').val(imageUrl);
        $('#icon #selectUrl').val(url);
        $('#iconDesc').val(description);
        $('#NSBIconId').val(id);
    }
    else {
        $('#NSBIconId').val('');
        $('#NSBIconBox').val('');
        $('#icon #selectUrl').val('');
        $('#iconDesc').val('');
        $("#icon input[name=selectRadio][value=1]").click();
    }

    $("#iconimgUrl").MallUpload({
        title: '图标：',
        imageDescript: '请上传115 * 115的图片',
        displayImgSrc: $('#NSBIconBox').val(),
        imgFieldName: "iconimgUrl",
        dataWidth: 7
    });

}


function generateNSBIcon() {
    //专题对象
    var slideImage = {
        id: null,
        description: null,
        imageUrl: null,
        url: null,
    };
    slideImage.id = $('#NSBIconId').val();
    slideImage.description = $('#iconDesc').val();
    slideImage.imageUrl = $("#iconimgUrl").MallUpload('getImgSrc');
    slideImage.url = $("#icon #selectUrl").val();
    slideImage.category = $("#icon input[type=radio]:checked").val();

    if (!slideImage.imageUrl)
        throw new Error('请上传图片');
    if (slideImage.description.length > 4)
        throw new Error('描述信息在4个字符以内');
    if (!slideImage.url)
        throw new Error('请选择跳转地址');
    if (slideImage.url.toLowerCase().indexOf('http://') < 0 && slideImage.url.toLowerCase().indexOf('https://') < 0 && slideImage.url.charAt(0) != '/')
        throw new Error('链接地址请以"http://"或"https://"开头');

    !slideImage.id && (slideImage.id = 0);
    return slideImage;
}



function submitNSBIcon() {
    var returnResult = false;
    var object;
    try {
        object = generateNSBIcon();
        if (!object.imageUrl)
            $.dialog.tips("请上传图片");
        else {
            var objectString = JSON.stringify(object);
            var loading = showLoading();
            $.ajax({
                type: "post",
                url: '/admin/NearShopBranch/AddNearShopBranchIcon',
                data: { id: object.id, description: object.description, imageUrl: object.imageUrl, url: object.category + "," + object.url },
                dataType: "json",
                async: false,
                success: function (result) {
                    loading.close();
                    if (result.success) {
                        returnResult = true;
                        $.dialog.tips('保存成功');
                        setTimeout(function () {
                            initNSBIconTable();
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

function chooseJumpUrl() {
    var selectValue = $("#icon input[type=radio]:checked").val();
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
        id: 'chooseSelectDialog',
        content: $('#chooseJumpUrl')[0],
        okVal: '保存',
        ok: function () {
            return saveChooseJumpUrl(jumpUrl, title);
        }
    });

    var finaly = -1;

    url = $("#icon #selectUrl").val();

    if (url.length > 0) {
        var topicId = url.split("/");
        finaly = topicId[topicId.length - 1];
    }

    //商品表格
    $("#jumpUrlGrid").MallDatagrid({
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
    $("#jumpUrlGrid th:last").html(desc);
}

function saveChooseJumpUrl(jumpurl, title) {
    var selectedId = $('input[name="topic"]:checked').attr('topicId');
    if (!selectedId)
        $.dialog.tips('请选择' + title);
    else {
        $("#icon #selectUrl").val(jumpurl.replace("{0}", selectedId));
    }
}