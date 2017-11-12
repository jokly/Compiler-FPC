lexer:
	python Tests\test.py -l

parser:
	python Tests\test.py -p

test: lexer parser

clean:
	git clean -x