function func(me, npc)
	local name = npc:name()
	if me:dialog(npc, string.format('안녕 내 이름은 %s야.', name)) == true then
		me:dialog(npc, 'NEXT')
	else
		me:dialog(npc, 'QUIT')
	end
end