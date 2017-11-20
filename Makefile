lexer:
	python Tests\test.py -l

parser:
	python Tests\test.py -p

semantic:
	python Tests\test.py -s

test: lexer parser semantic

clean:
	git clean -x