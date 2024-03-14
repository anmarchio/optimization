import os
from datetime import datetime
from os.path import join as p_join


def create_tex_list(path):
    tex = '\\begin{itemize}\n'
    for dirname in os.listdir(path):
        tex = tex + '\item ' + dirname + '\n'
    tex = tex + '\end{itemize}\n'
    return tex


def create_tex_report_details(path):
    tex = ''
    for dirname in os.listdir(path):
        tex = tex + '\subsection*{Report ' + dirname + '}\n\n'
        tex = tex + '\paragraph{Analyzer}\n'
        analyzer_path = p_join(path, dirname, 'Analyzer')
        if os.path.exists(analyzer_path):
            tex = tex + create_tex_list(analyzer_path) + '\n\n'
        else:
            tex = tex + '\\textbf{Empty}\n\n'

        tex = tex + '\paragraph{Config}\n'
        # config_path = p_join(path, dirname, 'Grid')
        # if os.path.exists(config_path):
        # else:
        tex = tex + '\\textbf{Empty}\n'

        tex = tex + '\paragraph{Grid}\n'
        grid_path = p_join(path, dirname, 'Grid')
        if os.path.exists(grid_path):
            tex = tex + create_tex_list(grid_path) + '\n\n'
        else:
            tex = tex + '\\textbf{Empty}\n\n'

        tex = tex + '\paragraph{Images}\n'
        images_path = p_join(path, dirname, 'Images')
        if os.path.exists(images_path):
            tex = tex + create_tex_list(images_path) + '\n\n'
        else:
            tex = tex + '\\textbf{Empty}\n\n'

        tex = tex + '\paragraph{Log}\n'
        log_path = p_join(path, dirname, 'Log', 'date.txt')
        if os.path.exists(log_path):
            f = open(log_path, 'r')
            tex = tex + '\\textbf{' + f.read() + '}\n\n'
        else:
            tex = tex + '\\textbf{Empty}\n\n'

    return tex


def generate_tex(source_path, target_path):
    tex_code = '\section*{PyCGP-SP Report ' + datetime.now().strftime('%m/%d/%Y, %H:%M:%S') + '}'
    """
    Loop through Folders and Create Report details
    """
    tex_code = tex_code + create_tex_report_details(source_path)
    f = open(p_join(target_path, 'report.tex'), 'w')
    f.write(tex_code)
    f.close()


def create_report():
    results_path = p_join(os.path.curdir, 'results')
    report_path = p_join(os.path.curdir, 'report')

    os.makedirs(report_path, mode=777, exist_ok=True)

    # Creates HTML file report/report.tex
    generate_tex(results_path, report_path) # TODO Implement tex generation and activate it


if __name__ == '__main__':
    create_report()
