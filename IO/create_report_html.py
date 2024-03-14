import json
import os
import webbrowser
from datetime import datetime
from os.path import join as p_join
import pydot


def create_list(path):
    html = '<ul>\n'
    for dirname in os.listdir(path):
        html += to_html_list_element(dirname)
    html += '</ul>\n'
    return html


def to_html_list_element(s):
    return '<li>\n' + s + '</li>\n'


def to_html_list(s):
    return '<ul>\n' + s + '</ul>\n'


def txt_to_html(txt: str):
    html = txt.replace('\n', '<br>').replace('\t', '&nbsp;&nbsp;&nbsp;&nbsp; ')
    return html


def make_collapsable(html, summary='&#x25BC;'):
    return f"""<details>
    <summary>
        {summary}: &#x25BC;
    </summary>
    <div>
            {html}
    </div>
</details>
"""


def insert_png(path):
    path = p_join('..', os.path.relpath(path))
    html = f'<img src="{path}" alt="{path}">'
    return html


def create_table_of_contents(path):
    html = f"""
    <h3 id="contents_{path}">Table of Contents</h3>
    <table class="table">
    <thead class="thead-dark">
        <tr>
            <th scope="col">#</th>
            <th scope="col">Source</th>
            <th scope="col">Run</th>
            <th scope="col">Lowest</th>
            <th scope="col">Highest</th>
        </tr>
    </thead>
    <tbody>
    """
    for i, dirname in enumerate(os.listdir(path)):
        if os.path.isdir(os.path.join(path, dirname)):
            src_path = os.path.join(path, dirname, "source.json")
            overview_path = os.path.join(path, dirname, "overview.json")
            lowestMCC = 0.0
            highestMCC = 0.0
            src_dirname = dirname
            if os.path.exists(overview_path):
                with open(overview_path) as of:
                    data = json.load(of)
                    for k in range(len(data)):
                        MCC_value = data[k]['Fitness']['MCC']
                        if MCC_value is not None:
                            if MCC_value > highestMCC:
                                highestMCC = MCC_value
                            if k == 0:
                                lowestMCC = MCC_value
                            elif MCC_value < lowestMCC:
                                lowestMCC = MCC_value
                if os.path.exists(src_path):
                    with open(src_path) as sf:
                        src_data = json.load(sf)
                        src_dirname = src_data[0]['trainingDataDirectory']
            html += make_content_row(i, source=src_dirname, dirname=dirname, lowest=lowestMCC,
                                     highest=highestMCC)  # TODO write fitness here

    html += '\n</tbody>\n</table><hr/>'
    return html


def get_col_bar(lowest):
    color = "red"
    if 0.3 < lowest < 0.5:
        color = "orange"
    elif 0.5 < lowest < 0.7:
        color = "yellow"
    elif 0.7 < lowest:
        color = "green"
    return f"""
        <div class="colorbar1">
            <span style="width: {lowest*100}%; height: 10px; display: block; float: lefT; background-color: {color};"></span>
        </div>
        <br />
        {lowest}
        """


def make_content_row(rowid, source, dirname, lowest, highest):
    get_col_bar(lowest)
    return f"""
        <tr><th scope="row"> {rowid}</th>
            <td>{source}</td> 
            <td><a href="#series{dirname}">{dirname}</a></td> 
            <td>
                {get_col_bar(lowest)}
            </td> 
            <td>
                {get_col_bar(highest)}
            </td>
        </tr>
        """


def json_to_html(json_object):
    # print(json_object)
    if type(json_object) is not dict:

        t = txt_to_html(str(json_object))
        if t.startswith('https://'):
            return f'<a href={t}>click here</a>'
        return t
    html = '<ul>'
    for id in json_object:
        html += to_html_list_element(f'{id}: {json_to_html(json_object[id])}')
    html += '</ul>'
    return html


def convert_file_to_html(file_path, filetypes=None):
    """
        Converts file in file_path if type is in filetypes.
    """
    if not os.path.exists(file_path):
        return to_html_list_element(f'<p>{file_path} not found!</p>')
    file_name = os.path.basename(file_path)
    if check_filetype(file_name, '.json', filetypes):
        with open(file_path) as file:
            j = json.load(file)
            html = json_to_html(j)
    elif check_filetype(file_name, '.txt', filetypes):
        with open(file_path) as file:
            html = txt_to_html(file.read())
    elif check_filetype(file_name, '.png', filetypes):
        html = insert_png(file_path)
    else:
        return ''
    return make_collapsable(html, file_name)


def check_filetypes(file_name, filetypes_to_check):
    for filetype in filetypes_to_check:
        if check_filetype(file_name, filetype):
            return True
    return False


def check_filetype(file_name, filetype_to_check, eligible_filetypes=None):
    """
        checks if file_name is of type filetype and filetype is one of the specified types.
    """
    return (eligible_filetypes is None or filetype_to_check in eligible_filetypes) and file_name.endswith(
        filetype_to_check)


'''
================== Config Log ==================
'''
def convert_directory_to_html_list(folder, file_types=['json']):
    if os.path.exists(folder):
        list_elements = ""
        for file_name in os.listdir(folder):
            file_path = p_join(folder, file_name)
            if check_filetypes(file_name, file_types):
                s = convert_file_to_html(file_path)
                list_elements += to_html_list_element(s)
        html = to_html_list(list_elements)
    else:
        html = '<p>Empty</p>\n'
    return html


def create_html_report_details(path, batch_name):
    html = ''
    default_dirs = [
        'Analyzer',
        'Config',
        'Grid',
        'Images',
        'Log'
    ]

    for dirname in default_dirs:

        html += f"""
        <h3 id=anchor{dirname}>Report {dirname}</h3>
        <p>
        <button class="btn btn-primary" type="button" data-toggle="collapse" data-target="#{dirname + batch_name}" aria-expanded="false" aria-controls="{dirname}">Show</button>
        <a class="btn btn-primary" data-toggle="contents" href="#contents_{path}" role="button" aria-expanded="false" aria-controls="contents">To Contents</a>
        </p>
        <div class="collapse" id="{dirname + batch_name}">
        <div class="card card-body">
        """
        if dirname == 'Analyzer':
            html += create_html_analyzer_section(p_join(path, dirname), batch_name)

        if dirname == 'Config':
            html += create_html_config_section(p_join(path, dirname))

        if dirname == 'Grid':
            html += create_html_grid_section(p_join(path, dirname))

        if dirname == 'Images' or dirname == 'Signals':
            html += create_html_items_section(p_join(path, dirname))

        if dirname == 'Log':
            log_path = p_join(path, dirname)
            if os.path.exists(log_path):
                html += create_html_log_section(p_join(path, dirname))

        html += f'<a href="#contents_{path}">Up</a>\n'
        html += '</div></div><hr/>\n'

    return html


def insert_images(image_path):
    html = ""
    i = 0
    max = 2
    for img in os.listdir(image_path):
        if check_filetypes(img, ['.jpg', '.png']):
            html += f"<img src='{os.path.join(os.pardir, image_path, img)}' width='25%' height='25%'><br />"
            i += 1
        if i > max:
            break
    return html


def create_html_items_section(items_path):
    html = ''
    if not os.path.exists(items_path):
        return html + '<p>Empty</p>\n'
    for iteration in os.listdir(items_path):
        folder = p_join(items_path, iteration)
        iteration_html = convert_directory_to_html_list(folder, ['.png'])
        html += to_html_list_element(
            f'Iteration {iteration}: {iteration_html}')
        html += insert_images(os.path.join(items_path, iteration))
    return make_collapsable(html, '<h4>Items</h4>')


'''
================== Grid Log ==================
'''
def to_svg(dot_graph):
    svg = ''
    if os.path.exists(dot_graph):
        with open(dot_graph, 'r') as f:
            graphs = pydot.graph_from_dot_data(f.read())
            graph = graphs[0]
            svg_data = graph.create_svg()
            svg += svg_data.decode("utf-8")
    return svg


def create_html_grid_section(grid_path):
    html = '<h4>Grid</h4>\n'
    if not os.path.exists(grid_path):
        return html + '<p>Empty</p>\n'
    iteration_html = ""
    for iteration in os.listdir(grid_path):
        iteration_html += to_html_list_element(
            make_collapsable(convert_directory_to_html_list(p_join(grid_path, iteration), [".json", ".txt"]),
                             f'Iteration {iteration}'))
        iteration_html += make_collapsable(to_svg(p_join(grid_path, iteration, "append_pipeline.txt")), f'append_pipeline.txt')
        iteration_html += make_collapsable(to_svg(p_join(grid_path, iteration, "pipeline.txt")), f'pipeline.txt')
    return html + to_html_list(iteration_html)


'''
================== Config Log ==================
'''
def create_html_config_section(config_folder):
    html = '<h4>Config</h4>\n'
    html += convert_directory_to_html_list(config_folder, ['.txt'])
    return html


def create_line_chart(data, title, batch_name, iteration):
    chart_id = title + batch_name + "_" + str(iteration)
    plot_html = f"""
        <div id="plot_{chart_id}" style="width:100%;max-width:700px"></div>
        <script>
    """

    plot_html += 'var xArray = ['
    for x in range(len(data)):
        plot_html += str(data[x]['Generation'])
        if x < len(data) - 1:
            plot_html += ','
    plot_html += "];\n"

    plot_html += 'var yArray = ['
    for y in range(len(data)):
        plot_html += str(data[y][title])
        if y < len(data) - 1:
            plot_html += ','
    plot_html += "];\n"

    plot_html += """
        // Define Data
        var data = [{
            x: xArray,
            y: yArray,
            mode: "lines",
            type: "scatter"
        }];

        // Define Layout
        var layout = {
            xaxis: {range: [0, 10], title: "Generation"},
            yaxis: {range: [-1, 1], title: "MCC Fitness"},
            title: "Fitness Development"
        };
    
        // Display using Plotly
        """
    plot_html += f"""
        Plotly.newPlot("plot_{chart_id}", data, layout);
        """
    plot_html += '</script>'
    return plot_html


def create_data_plot(folder, file_name, plot_title, batch_name, iteration):
    plot_html = ''
    if os.path.exists(os.path.join(folder, file_name)):
        with open(os.path.join(folder, file_name), 'r') as f:
            data = json.load(f)
            plot_html += create_line_chart(data, plot_title, batch_name, iteration)
            plot_html += '<table class ="table">'
            plot_html += '<tr><td>Generation</td>'
            for i in range(len(data)):
                plot_html += '<td>' + str(data[i]['Generation']) + '</td>'
            plot_html += '</tr><tr><td>' + plot_title + '</td>'
            for i in range(len(data)):
                plot_html += '<td>' + str(data[i][plot_title]) + '</td>'
            plot_html += '</tr></table>'
    return plot_html


'''
================== Analyzer Log ==================
'''
def create_html_analyzer_section(analyzer_folder, batch_name):
    html = '<h4>Analyzer</h4>\n'
    if not os.path.exists(analyzer_folder):
        return html + '<p>Empty</p>\n'
    html += '<p>'
    for iteration in os.listdir(analyzer_folder):
        html += '<h4 style="background-color:#3379b7;color:white;">Run No. ' + str(iteration) + '</h4>'
        folder = p_join(analyzer_folder, iteration)

        html += create_data_plot(folder, 'AvgOffspringFit.json', 'AverageOffspringFitness', batch_name, iteration)
        html += create_data_plot(folder, 'AvgPopulationFit.json', 'AveragePopulationFitness', batch_name, iteration)
        html += create_data_plot(folder, 'BestIndividualFit.json', 'BestIndividualFitness', batch_name, iteration)

        if os.path.exists(os.path.join(folder, 'individual_evaluation_log.json')):
            with open(os.path.join(folder, 'individual_evaluation_log.json'), 'r') as f:
                data = json.load(f)
                html += make_collapsable(data, 'loader_evaluation_log.json')
        if os.path.exists(os.path.join(folder, 'loader_evaluation_log.json')):
            with open(os.path.join(folder, 'loader_evaluation_log.json'), 'r') as f:
                data = json.load(f)
                html += make_collapsable(data, 'loader_evaluation_log.json')
    html += '</table>'
    return html


'''
================== Log ==================
'''
def create_html_log_section(folder):
    html = '<h4>Log</h4>\n'
    if not os.path.exists(folder):
        return html + '<p>Empty</p>\n'
    html_list_elements = ''
    for file_name in os.listdir(folder):
        file_path = p_join(folder, file_name)
        file_html = convert_file_to_html(file_path, ['.txt'])
        html_list_elements += to_html_list_element(file_html)
    return html + to_html_list(html_list_elements)


def create_style():
    return """
    <style>
    details > summary {
      padding: 4px;
      width: 200px;
      background-color: #eeeeee;
      border: 1px solid;
      border-radius: 5px;
      margin-bottom: 2px;
      box-shadow: 1px 1px 2px #bbbbbb;
      cursor: pointer;
    }
    
    details > div {
      background-color: #eeeeee;
      border: 1px solid;
      border-radius: 4px;
      padding: 4px;
      margin-bottom: 2px;
      box-shadow: 1px 1px 2px #bbbbbb;
    }
    </style>
    """


def create_html_head():
    title = 'CGP-IP Report'
    html_code = f"""
    <!doctype html>
    <html lang="en">
    <head>
        <meta charset="utf-8">
        <title>{title}</title>
        <meta name="description" content="Generated PyCGP Report"><meta name ="author" content="PyCGP">{create_style()} 
    </head>
    <!-- Latest compiled and minified CSS -->
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css">
    <!-- jQuery library --><script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
    <!-- Latest compiled JavaScript -->
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/js/bootstrap.min.js"></script>
    <script src="https://cdn.plot.ly/plotly-latest.min.js"></script>
    <body>
        <div class="container">
        <h1 class="display-1">{title}</h1>
        <p>Generated {datetime.now().strftime('%m/%d/%Y, %H:%M:%S')}</p>
        """
    return html_code


def generate_html(source_path, target_path):
    html_code = create_html_head()

    # Insert Table of Contents
    html_code += create_table_of_contents(source_path)

    for batch_name in os.listdir(source_path):
        html_code += create_html_of_test_batch(source_path, batch_name)
    html_code += '</div>\n</body>\n</html>'
    with open(p_join(target_path, 'index.html'), 'w') as f:
        f.write(html_code)
        webbrowser.open(f.name)


def create_html_of_test_batch(source_path, test_batch_name):
    path = p_join(source_path, test_batch_name)
    if not os.path.isdir(path):
        return ''

    # Loop through Folders and Create Report details
    html_report_details = create_html_report_details(path, test_batch_name)

    html = f"""
        <h2 id="series{test_batch_name}">Testseries of {test_batch_name}</h2>
        <button class="btn btn-primary" type="button" data-toggle="collapse" data-target="#{test_batch_name}" aria-expanded="false" aria-controls="{test_batch_name}">Show</button>
        </h2>
        <div class="collapse" id="{test_batch_name}">
        <div class="card card-body">
        {html_report_details}
        </div></div><hr/>        
    """

    return html


def create_report():
    results_path = p_join(os.path.curdir, 'results')
    report_path = p_join(os.path.curdir, 'report')

    os.makedirs(report_path, mode=777, exist_ok=True)

    # Creates HTML file report/index.html
    generate_html(results_path, report_path)


if __name__ == '__main__':
    create_report()
