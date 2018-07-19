function daochu() {
    var parames = new Array();
    parames.push({ name: "daochu", value: "daochu" });
    var div = document.getElementById("search");
    var input = div.getElementsByTagName("input");
    var select = div.getElementsByTagName("select");
    for (var i = 0; i < input.length; i++) {
        parames.push({ name: input[i].name, value: input[i].value });
    }
    for (var i = 0; i < select.length; i++) {
        parames.push({ name: select[i].name, value: select[i].value });
    }
    var temp_form = document.createElement("form");
    temp_form.action = window.location.href;
    temp_form.target = "_blank";
    temp_form.method = "post";
    temp_form.style.display = "none";
    for (var item in parames) {
        var opt = document.createElement("textarea");
        opt.name = parames[item].name;
        opt.value = parames[item].value;
        temp_form.appendChild(opt);
    }
    document.body.appendChild(temp_form);
    temp_form.submit();
    return false;
}