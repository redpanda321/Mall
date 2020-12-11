(function () {
    var templateCover = '<div class="template-cover"><label class="j_up">上移</label><label class="j_down">下移</label><label class="j_del">删除</label></div>';
	
	var _vm;
	
    function initCover() {
        $('.template-node:not(.template-inited)').each(function () {
            var temp = templateCover;
            var $this = $(this);
            if ($('>.template-cover', this).length > 0)
                return;

            var updown = $this.attr('updown');
            if (updown == 0) {
                temp = '<div class="template-cover"><label>删除</label></div>';
            }
			//temp=temp.replace('$titlePlace$',$this.attr('t-title')).replace('$zindex$',$this.attr('z-index')||0);
            
            $this.addClass('template-inited').append(temp);
        });
        initStates();//初始化第一个上移最后一个下移禁用状态
    }

    $(function () {
        initCover();
        var html = '';
        $(document).on('click', '.template-cover .j_up', function (event) {
            var title = $(this).closest('.template-node');//在执行上移操作之前保留
            var _title = $(this).parent().parent().prev();


            var index = $(this).closest('.template-node').attr('index');
            //先删除，再添加，删之前先保存
            var current = _vm.news[index];
            if (index - 1 >= 0) {
                $(this).removeClass("disabled");
                _vm.news.splice(index, 1);
                _vm.news.splice(index - 1, 0, current); //从第0个位置开始插入 
            } else {
                $(this).addClass("disabled");
            }
            $(this).parent().attr("title", _title.attr("t-title"));//dui
            _title.find(".template-cover").attr("title", title.attr("t-title"));
            event.stopPropagation();
            return false;
        });
        $(document).on('click', '.template-cover .j_down', function (event) {
            var index = $(this).closest('.template-node').attr('index');
            var title = $(this).closest('.template-node');//在执行上移操作之前保留
            var _title = $(this).parent().parent().next();

            var current = _vm.news[index];
            var _index = parseInt(index);
            _vm.news.splice(_index, 1);
            _vm.news.splice(_index + 1, 0, current);
            if (_index + 1 == _vm.news.length) {
                $(this).addClass("disabled");
            } else {
                $(this).removeClass("disabled");
            }
            $(this).parent().attr("title", _title.attr("t-title"));//dui
            _title.find(".template-cover").attr("title", title.attr("t-title"));

            event.stopPropagation();
            return false;
        });
        $(document).on('click', '.template-cover .j_del', function (event) {
            var obj = $(this);
            $.jBox.show({
                title: "提示",
                content: "删除后将不可恢复，是否继续？",
                btnOK: {
                    onBtnClick: function (jbox) {
                        $.jBox.close(jbox);
                        var index = obj.closest('.template-node').attr('index');
                        $('.template-node.template-inited').removeClass('template-inited');
                        $('.template-node>.template-cover').remove();
                        _vm.news.splice(index, 1);
                        event.stopPropagation();
                    }
                }
            });
            return false;
        }).on('click', '.template-cover', function (event) {
            coverClick.call(event.target);
            event.stopPropagation();
            return false;
        });
        _vm = new Vue({
            el: '#content',
            data: window.top._data.vue,
            mounted: function () {
                initCover();
                initHotSearch();//页面vue渲染完成后加载
                $(".j_hideAd").show();
                $(".lazyload").scrollLoading();
            },
            updated: function () {
                $(".j_hideAd").show();
                $(".lazyload").scrollLoading();
                initCover();
                initHotSearch();//页面vue渲染完成后加载
            }
        });
        window._vm = _vm;
    });

    window.top._addNewItem = function (data) {
        if (data.name == 'advertisement') {
            _vm.advertisement.hide = false;
        }
        else {
            _vm.news.push(JSON.parse(JSON.stringify(data)));
            $('html,body').animate({ scrollTop: $('#j_footer').offset().top }, 50);
        }
        initStates();//新增模块时，需要变更上下移动的禁用状态
    };

    window.top._getDocument = function () {
        var list = [];
        $('.template-node .template-cover').each(function () {
            var item = {};
            var parent = $(this).closest('.template-node');
            item.parent = parent;
            item.child = this;
            $(this).remove();

            item.class = ['template-node'];
            parent.removeClass(item.class[0]);

            var c = 'template-nodel';
            if (parent.hasClass(c)) {
                parent.removeClass(c);
                item.class.push(c);
            }

            c = 'template-inited';
            if (parent.hasClass(c)) {
                parent.removeClass(c);
                item.class.push(c);
            }

            list.push(item);
        });

        var html = document.documentElement.innerHTML;

        for (var i = 0; i < list.length; i++) {
            var item = list[i];
            item.parent.append(item.child);
            for (var j = 0; j < item.class.length; j++) {
                item.parent.addClass(item.class[j]);
            }
        }

        return html;
    };

    function coverClick() {
        var $this = $(this);
        var templateNode = $this.closest('.template-node');
        //var isfooter = templateNode.attr('isfooter');
        //if (isfooter == "1") {
        //    $.dialog({
        //        title: '底部信息',
        //        lock: true,
        //        id: 'footer',
        //        content: ['<div class="dialog-form"><div class="form-group"><lable>请在网站设置</lable><a href="/Admin/ArticleCategory/management" _blank="target">文章分类</a><lable>的“底部帮助”中新增或编辑下级分类，进行</lable><a href="/Admin/Article/management" _blank="target">文章管理</a><lable>或</lable><a href="/Admin/Article/Add" _blank="target">新增文章</a></div></div>'].join(''),
        //        padding: '0 40px'
        //    });
        //    return;
        //}
        var name = templateNode.attr('name'); 
        if (name == "") {
            return;
        }
        if (name == 'customHtml') {
            window.top.setEditor();
        } else {
            window.top.saveEditor();
        }
        var title = templateNode.attr('t-title');
        var componentData = window.top._componentData[name];
        //if (name == 'advertisement') {
        //    window.top._componentData["advertisement"].hide = false;//每次点开即为显示
        //}
        var vmData = _vm[name];
        if (vmData == null) {
            var newsIndex = templateNode.attr('index');
            if (newsIndex)
                vmData = _vm.news[newsIndex];
        }

        if (componentData == null && vmData != null) {
            componentData = JSON.parse(JSON.stringify(vmData));
            //window.top._componentData.clickCoverData = {};
            mapper(window.top._componentData.clickCoverData, componentData, true);
            componentData = window.top._componentData.clickCoverData;
        }

        window.top._vm.showDialog = true;
        window.top._vm.dialogTitle = title;
        window.top._vm.dialogView = name;

        if (vmData) {
            window.top._dialogOk = function () {
                if (vmData) {
                    mapper(vmData, componentData);
                }
            };
        }
    }

    function mapper(target, source, copyProperty) {
        //copyProperty:boolean 是否拷贝属性 true:表示target没有某属性但source中有时，将会拷贝过去
        if (utility.isArray(source)) {
            for (var i = 0; i < source.length; i++) {
                target.push(JSON.parse(JSON.stringify(source[i])));
            }
        } else {
            for (var property in source) {
                if (copyProperty != true && target[property] == undefined)
                    continue;

                var value = source[property];
                if (utility.isArray(value)) {
                    var temp = target[property];
                    if (temp == null || temp == undefined) {
                        temp = [];
                        target[property] = temp;
                    } else {
                        temp.clear();
                    }
                    mapper(temp, value);
                }
                else
                    target[property] = value;
            }
        }
    }

    var gettype = Object.prototype.toString;
    var utility = {
        isObj: function (o) {
            return gettype.call(o) == "[object Object]";
        },
        isArray: function (o) {
            return gettype.call(o) == "[object Array]";
        },
        isNULL: function (o) {
            return gettype.call(o) == "[object Null]";
        }
    }

    function initStates() {
        $(".j_up").removeClass("disabled");
        $(".j_down").removeClass("disabled");
        $(".j_up").first().addClass("disabled");
        $(".j_down").last().addClass("disabled");
    }

    function initHotSearch() {
        var hotSearch = window.top._data.vue.search.hotKeyWords;
        if (hotSearch != '' && typeof (hotSearch) != "undefined") {
            var arrHot = hotSearch.split(',');
            if (arrHot.length > 0) {
                var content = '';
                for (var i = 0; i < arrHot.length; i++) {
                    var link = "location.href=encodeURI($(this).attr('url'))";
                    content += '<a  onclick="' + link + '" url="/search/searchAd?keywords=' + arrHot[i] + '" keyword="' + arrHot[i] + '">' + arrHot[i] + '</a>';
                }
                $(".hot-search").html(content);
            }
        }
    }
})();