function func(me, npc)
	local name = npc:name()
	if me:dialog(npc, string.format('안녕 내 이름은 %s야.', name)) == true then
		me:dialog(npc, '뭘봐')
	end
end