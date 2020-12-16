function func(me, item)
	local name = npc:name()
	me:dialog(item, '안녕')
	me:dialog(item, string.format('니가 사용한 아이템 이름은 %s야', name))
	me:dialog(item, '그럼 이만')
	me:dialog(item, '뿅')
end