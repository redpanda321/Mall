function LoadWXInfos()
{
    $.post( "/m-weixin/shopbonus/GetOtherReceive", { id: $( "#grant" ).val() }, function ( result )
    {
        var html = "";
        for (var i = 0; i < result.data.length ; i++)
        {
            html += '<li class="clearfix">';
            html += '<div class="head-portrait"><img src="' + result.data[i].HeadImg + '"></div>';
            html += '<div class="info">';
            html += '<div class="info-c"><span class="name">' + result.data[i].Name + '</span><time>' + result.data[i].ReceiveTime + '</time></div>';
            html += '<p>' + result.data[i].Copywriter + '</p>';
            html += '</div>';
            html += '<div class="money">' + result.data[i].Price + '元</div>';
            html += '</li>';
        }

        $( ".mid" ).html( html );

    } );
}