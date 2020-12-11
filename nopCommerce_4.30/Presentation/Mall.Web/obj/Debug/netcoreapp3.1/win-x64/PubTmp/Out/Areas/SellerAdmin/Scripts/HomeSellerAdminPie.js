$(function () {
    $("#productsNumberDiv").text(productsNumberIng);
    $("#useSpaceDiv").text(useSpace + "M");
    require.config({ paths: { echarts: '/Scripts' } });
    require(['echarts', 'echarts/chart/pie'], loadChart);
    initLogo();
    setLogo();//设置店铺Logo
});

function loadChart(echarts) {
    //店铺效果分析
    var shopProductPie = echarts.init(document.getElementById('shopProductPie'));
    var shopImagePie = echarts.init(document.getElementById('shopImagePie'));
    shopProductPie.showLoading({ text: '正在加载图表...', effect: 'bubble', textStyle: { fontSize: 20 } });
    shopImagePie.showLoading({ text: '正在加载图表...', effect: 'bubble', textStyle: { fontSize: 20 } });
    var labelTop = {
        normal: {
            label: {
                show: true,
                position: 'bottom',
                formatter: '{b}',
                textStyle: {
                    baseline: 'top'
                }
            },
            labelLine: {
                show: false
            }
        }
    };
    var labelBottom = {
        normal: {
            color: '#fff',
            label: {
                show: true,
                position: 'center'
            },
            labelLine: {
                show: false
            }
        },
        emphasis: {
            color: '#fff'
        }
    };
    var shopProductPieOption = {
        legend: {
            x: 'right',
            y: 'center',
            data: [

            ]
        },
        title: {
            text: '',
            subtext: '',
            x: 'center'
        },
        toolbox: {
            show: false,
            feature: {
                dataView: { show: false, readOnly: false },
                magicType: {
                    show: true,
                    type: ['pie', 'funnel'],
                    option: {
                        funnel: {
                            width: '20%',
                            height: '30%',
                            itemStyle: {
                                normal: {
                                    label: {
                                        formatter: function (params) {
                                            return 'other\n' + params.value + '%\n'
                                        },
                                        textStyle: {
                                            baseline: 'middle'
                                        }
                                    }
                                },
                            }
                        }
                    }
                },
                restore: { show: false },
                saveAsImage: { show: false }
            }
        },
        series: [
            {
                pieNumber: productsNumberIng,
                type: 'pie',
                center: ['50%', '45%'],
                radius: [43, 44],
                x: '0%', // for funnel
                itemStyle: {
                    normal: {
                        color: 'red',
                        label: {
                            formatter: function (params) {
                                return productsPercentage + '%'
                            },
                            textStyle: {
                                baseline: 'top'
                            }
                        }
                    },
                },
                data: [
                    { name: 'productsNumberIng', value: (productsNumberIng - productsNumber), itemStyle: labelBottom },
                    { name: 'ProductsNumber', value: productsNumber, itemStyle: labelTop }

                ]
            }
        ]
    };
    var shopImagePieOption = {
        legend: {
            x: 'right',
            y: 'center',
            data: [

            ]
        },
        title: {
            text: '',
            subtext: '',
            x: 'center'
        },
        toolbox: {
            show: false,
            feature: {
                dataView: { show: false, readOnly: false },
                magicType: {
                    show: true,
                    type: ['pie', 'funnel'],
                    option: {
                        funnel: {
                            width: '20%',
                            height: '30%',
                            itemStyle: {
                                normal: {
                                    label: {
                                        formatter: function (params) {
                                            return 'other\n' + params.value + '%\n'
                                        },
                                        textStyle: {
                                            baseline: 'middle'
                                        }
                                    }
                                },
                            }
                        }
                    }
                },
                restore: { show: false },
                saveAsImage: { show: false }
            }
        },
        series: [
            {
                pieNumber: useSpace,
                type: 'pie',
                center: ['50%', '45%'],
                radius: [43, 44],
                x: '0%', // for funnel
                itemStyle: {
                    normal: {
                        color: 'red',
                        label: {
                            formatter: function (params) {
                                return useSpaceingPercentage + '%'
                                // return 500 - params.value + '%'
                            },
                            textStyle: {
                                baseline: 'top'
                            }
                        }
                    },
                },
                data: [
                    { name: 'useSpaceing    ', value: (useSpace - useSpaceing), itemStyle: labelBottom },
                    { name: 'useSpace', value: useSpaceing, itemStyle: labelTop }

                ]
            }
        ]
    };
    shopProductPie.hideLoading();
    shopImagePie.hideLoading();
    shopProductPie.setOption(shopProductPieOption);
    shopImagePie.setOption(shopImagePieOption);
}
function setLogo() {
    $('.j_logo_area').click(function () {
        $.dialog({
            title: 'LOGO设置',
            lock: true,
            width: 400,
            id: 'logoArea',
            content: document.getElementById("logosetting"),
            padding: '10px 30px',
            okVal: '保存',
            ok: function () {
                var logosrc = $("input[name='Logo']").val();
                if (logosrc == "") {
                    $.dialog.tips("请上传一张LOGO图片！");
                    return false;
                }
                var loading = showLoading();
                $.post('/selleradmin/home/setlogo', { logo: logosrc },
                    function (data) {
                        loading.close();
                        if (data.success) {
                            $.dialog.succeedTips("LOGO修改成功！");
                            $("input[name='Logo']").val(data.logo + '?r=' + new Date().getMilliseconds());
                            $(".j_logo_area").attr("src", data.logo + '?r=' + new Date().getMilliseconds());
                        }
                        else { $.dialog.errorTips("LOGO修改失败！") }
                    });
            }
        });
    });

}
function initLogo() {
    $("#uploadImg").MallUpload(
      {
          displayImgSrc: logo,
          imgFieldName: "Logo",
          title: 'LOGO:',
          imageDescript: '160*160',
          dataWidth: 8
      });
}