function func(me, npc)
	local items = me:items()
	local message = '니가 가진 아이템들 목록이야.\n'
	for i, item in pairs(items) do
		message = message .. item:name() .. '\n'
	end
	me:dialog(npc, message)


	local item_name = '포션.체력회복'
	me:dialog(npc, string.format('널 위해 \'%s\'를 줄게', item_name))

	local item = mkitem(item_name)
	me:item_add(item)

	-- local name = npc:name()
	-- if me:dialog(npc, string.format('안녕 내 이름은 %s야. 선택할래??? (dialog/next or dialog/quit)', name)) == true then
	-- 	local menu = {'1번메뉴', '2번메뉴', '3번메뉴', '4번메뉴'}
	-- 	local selection = me:dialog_list(npc, '1~4중에 골라봐 (dialog/select {index})', menu)
	-- 	me:dialog(npc, string.format('니 선택은 %s야.', menu[selection]))
	-- else
	-- 	me:dialog(npc, '싫음 말고 ㅎ')
	-- end
end