// JavaScript source code
$(function () {
    InitBrands();
    UploadInit($("#Logo").val());
    InitBrandLetter($("#AuthCertificate").val());
    InitBrandAuthPic($("#AuthCertificate").val());
    $("#band_name").blur(function () {
        var name = $('#band_name').val();
        var reg = /^[a-zA-Z0-9\u4e00-\u9fa5]+$/;
        if (name.length > 30) {
            $("#brandNameTip").text("请输入小于30个字符！");
            $('#band_name').css({ border: '1px solid #f60' });
            $('#band_name').focus();
            return false;
        }
        //if (reg.test(name) == false) {
        //    $("#brandNameTip").text("品牌名称必须是中文，字母，数字！");
        //    $('#band_name').css({ border: '1px solid #f60' });
        //    $('#band_name').focus();
        //    return false;
        //}
        //else {
        //    $("#brandNameTip").text("");
        //    $('#band_name').css({ border: '1px solid #ccc' });
        //}

        $.ajax({
            type: 'post',
            url: './IsExist',
            data: { name: name },
            dataType: "json",
            async: false,
            success: function (data) {
                if (data.success == true) {
                    $("#brandNameTip").text("该品牌已存在，请选择申请已有品牌！");
                    $('#band_name').css({ border: '1px solid #f60' });
                    $('#band_name').focus();
                }
                else {
                    $("#brandNameTip").text("");
                    $('#band_name').css({ border: '1px solid #ccc' });
                }
            }
        });
    });

    $("#band_remark").blur(function () {
        var band_remark = $("#band_remark").val();
        if (band_remark.length > 0) {
            if (band_remark.length > 200) {
                $("#band_remarkTip").text("备注不能超过200个字符！");
                $('#band_remark').css({ border: '1px solid #f60' });
                $('#band_remark').focus();
                return false;
            }
        }
    });

    $("#band_des").blur(function () {
        var band_des = $("#band_des").val();
        if (band_des.length > 0) {
            if (band_des.length > 200) {
                $("#band_desTip").text("简介不能超过200个字符！");
                $('#band_des').css({ border: '1px solid #f60' });
                $('#band_des').focus();
                return false;
            }
        }
    });
});


function EditApply() {
    var ajaxGet = function (url, d, fn) {
        var loading = showLoading();
        $.ajax({
            type: "POST",
            url: url,
            data: d,
            dataType: "json",
            success: function (data) {
                loading.close();
                if (data.success == true) {
                    $.dialog.succeedTips('重新编辑品牌申请成功,正在等待平台审核!', function () {
                        window.location.href = "/Selleradmin/Brand/Management";
                    });
                } else {
                    $.dialog.errorTips(data.msg)
                }
            }
        });
    };

    var id = $("#Id").val();
    var brandId = $("#BrandId").val();
    var mode = $("input[name='mode']").val();
    if (mode == "1") {
        var ids = [];
        $(".choose-brand .checkbox-inline").each(function () {
            var id = $(this).attr("value");
            var name = $(this).text();
            ids.push(id);
        });
        if (ids.length == 0) {
            $.dialog.errorTips("请选择品牌");
            return false;
        }

        var brandLetter = $('#brandLetter').MallUpload('getImgSrc').toString(),
        remark = $('#band_remark').val();

        if (brandLetter == "") {
            $.dialog.errorTips("请上传品牌授权证书！");
            return false;
        }

        if (remark.length > 0) {
            if (remark.length > 200) {
                $("#band_remarkTip").text("备注不能超过200个字符！");
                $('#band_remark').css({ border: '1px solid #f60' });
                $('#band_remark').focus();
                return false;
            }
        }

        if (!brandLetter) {
            $('#brandLetter').find('input').css({ border: '1px solid #f60' });
        } else {
            $('#brandLetter').find('input').css({ border: '1px solid #ccc' });
        }
        if (brandLetter == "") return false;
        ajaxGet('EditApply', { "Id": id, "BrandId": ids[0], "BrandMode": mode, "BrandAuthPic": brandLetter, "Remark": remark });
    }
    else {
        var pic = $("#brandPic").MallUpload('getImgSrc').toString(),
        authpic = $("#brandAuthPic").MallUpload('getImgSrc').toString(),
        name = $('#band_name').val(),
        des = $('#band_des').val();
        var reg = /^[a-zA-Z0-9\u4e00-\u9fa5]+$/;

        if (pic == "") {
            $.dialog.errorTips("请上传品牌LOGO！");
            return false;
        }
        if (authpic == "") {
            $.dialog.errorTips("请上传品牌授权证书！");
            return false;
        }

        if (!des) {
            $('#band_des').css({ border: '1px solid #f60' });
            $('#band_des').focus();
        } else {
            if (band_des.length > 200) {
                $("#band_desTip").text("简介不能超过200个字符！");
                $('#band_des').css({ border: '1px solid #f60' });
                $('#band_des').focus();
                return false;
            } else {
                $('#band_des').css({ border: '1px solid #ccc' });
            }
        }
        if (!authpic) {
            $('#brandAuthPic').find('input').css({ border: '1px solid #f60' });
        } else {
            $('#brandAuthPic').find('input').css({ border: '1px solid #ccc' });
        }
        if (!pic) {
            $('#brandPic').find('input').css({ border: '1px solid #f60' });
        } else {
            $('#brandPic').find('input').css({ border: '1px solid #ccc' });
        }
        if (!name || name.length > 30) {
            $('#band_name').css({ border: '1px solid #f60' });
            $('#band_name').focus();
        } else {
            $('#band_name').css({ border: '1px solid #ccc' });
        }

        if (!name || name.length > 30 || !pic || !des || !authpic) return false;

        $.ajax({
            type: 'post',
            url: './IsExist',
            data: { name: name },
            dataType: "json",
            async: false,
            success: function (data) {
                if (data.success == true) {
                    $.dialog.tips("该品牌已存在，请选择申请已有品牌！");
                    return false;
                }
                else {
                    ajaxGet('EditApply', { "Id": id, "BrandMode": mode, "BrandName": name, "BrandLogo": pic, "BrandAuthPic": authpic, "BrandDesc": des });
                }
            }
        });

    }


}